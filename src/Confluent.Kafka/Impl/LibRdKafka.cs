// Copyright 2015-2016 Andreas Heider,
//           2016-2023 Confluent Inc. 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Derived from: rdkafka-dotnet, licensed under the 2-clause BSD License.
//
// Refer to LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Confluent.Kafka.Admin;
using Confluent.Kafka.Internal;
using System.Reflection;
#if NET462
using System.ComponentModel;
#endif



namespace Confluent.Kafka.Impl
{
    internal static unsafe partial class Librdkafka
    {
        const int RTLD_NOW = 2;

        internal enum DestroyFlags
        {
            /*!
            * Don't call consumer_close() to leave group and commit final offsets.
            *
            * This also disables consumer callbacks to be called from rd_kafka_destroy*(),
            * such as rebalance_cb.
            *
            * The consumer group handler is still closed internally, but from an
            * application perspective none of the functionality from consumer_close()
            * is performed.
            */
            RD_KAFKA_DESTROY_F_NO_CONSUMER_CLOSE = 0x8
        }

        internal enum AdminOp
        {
            Any = 0,
            CreateTopics = 1,
            DeleteTopics = 2,
            CreatePartitions=  3,
            AlterConfigs = 4,
            DescribeConfigs = 5,
            DeleteRecords = 6,
            DeleteGroups = 7,
            DeleteConsumerGroupOffsets = 8,
            CreateAcls = 9,
            DescribeAcls = 10,
            DeleteAcls = 11,
            ListConsumerGroups = 12,
            DescribeConsumerGroups = 13,
            ListConsumerGroupOffsets = 14,
            AlterConsumerGroupOffsets = 15,
            IncrementalAlterConfigs = 16,
            DescribeUserScramCredentials = 17,
            AlterUserScramCredentials = 18,
            DescribeTopics = 19,
            DescribeCluster = 20,
            ListOffsets = 21,
            ElectLeaders = 22,
        }

        public enum EventType : int
        {
            None = 0x0,
            DR = 0x1,
            Fetch = 0x2,
            Log = 0x4,
            Error = 0x8,
            Rebalance = 0x10,
            Offset_Commit = 0x20,
            Stats = 0x40,
            CreateTopics_Result = 100,
            DeleteTopics_Result = 101,
            CreatePartitions_Result = 102,
            AlterConfigs_Result = 103,
            DescribeConfigs_Result = 104,
            DeleteRecords_Result = 105,
            DeleteGroups_Result = 106,
            DeleteConsumerGroupOffsets_Result = 107,
            CreateAcls_Result = 0x400,
            DescribeAcls_Result = 0x800,
            DeleteAcls_Result = 0x1000,
            ListConsumerGroups_Result = 0x2000,
            DescribeConsumerGroups_Result = 0x4000,
            ListConsumerGroupOffsets_Result = 0x8000,
            AlterConsumerGroupOffsets_Result = 0x10000,
            IncrementalAlterConfigs_Result = 0x20000,
            DescribeUserScramCredentials_Result = 0x40000,
            AlterUserScramCredentials_Result = 0x80000,
            DescribeTopics_Result = 0x100000,
            DescribeCluster_Result = 0x200000,
            ListOffsets_Result = 0x400000,
            ElectLeaders_Result = 0x800000,
        }

        // Minimum librdkafka version.
        const long minVersion = 0x010502ff;

        // Maximum length of error strings built by librdkafka.
        internal const int MaxErrorStringLength = 512;

        private static class WindowsNative
        {
            [Flags]
            public enum LoadLibraryFlags : uint
            {
                DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
                LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
                LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
                LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
                LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
                LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
                LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
                LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
                LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
                LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
                LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
            }

            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpFileName);

            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, String procname);
        }

        private static class PosixNative
        {
            [DllImport("libdl")]
            public static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl")]
            public static extern IntPtr dlerror();

            [DllImport("libdl")]
            public static extern IntPtr dlsym(IntPtr handle, String symbol);

            public static string LastError
            {
                get
                {
                    // TODO: In practice, the following is always returning IntPtr.Zero. Why?
                    IntPtr error = dlerror();
                    if (error == IntPtr.Zero)
                    {
                        return "";
                    }
                    return Marshal.PtrToStringAnsi(error);
                }
            }
        }

        static object loadLockObj = new object();
        static bool isInitialized = false;

        public static bool IsInitialized
        {
            get
            {
                lock (loadLockObj)
                {
                    return isInitialized;
                }
            }
        }

        /// <summary>
        ///     Attempt to load librdkafka.
        /// </summary>
        /// <returns>
        ///     true if librdkafka was loaded as a result of this call, false if the
        ///     library has already been loaded.
        ///
        ///     throws DllNotFoundException if librdkafka could not be loaded.
        ///     throws FileLoadException if the loaded librdkafka version is too low.
        ///     throws InvalidOperationException on other error.
        /// </returns>
        public static bool Initialize(string userSpecifiedPath)
        {
            lock (loadLockObj)
            {
                if (isInitialized)
                {
                    return false;
                }

#if NET462

                if (!MonoSupport.IsMonoRuntime)
                {
                    LoadNetFrameworkBindings(userSpecifiedPath);
                }
                else
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        LoadLinuxBindings(userSpecifiedPath);
                    }
                    else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                    {
                        LoadOSXBindings(userSpecifiedPath);
                    }
                    else
                    {
                        // Assume other PlatformId enum cases are Windows based
                        // (at the time of implementation, this is the case).
                        LoadNetFrameworkBindings(userSpecifiedPath);
                    }
                }

#else

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    LoadNetStandardBindings(userSpecifiedPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    LoadOSXBindings(userSpecifiedPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    LoadLinuxBindings(userSpecifiedPath);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported platform: {RuntimeInformation.OSDescription}");
                }

#endif

                if ((long)version() < minVersion)
                {
                    throw new FileLoadException($"Invalid librdkafka version {(long)version():x}, expected at least {minVersion:x}");
                }

                isInitialized = true;
                return true;
            }
        }


#if NET462
        private static void LoadNetFrameworkBindings(string userSpecifiedPath)
        {
            string path = userSpecifiedPath;
            if (path == null)
            {
                // in net45, librdkafka.dll is not in the process directory, we have to load it manually
                // and also search in the same folder for its dependencies (LOAD_WITH_ALTERED_SEARCH_PATH)
                var is64 = IntPtr.Size == 8;
                var baseUri = new Uri(Assembly.GetExecutingAssembly().GetName().EscapedCodeBase);
                var baseDirectory = Path.GetDirectoryName(baseUri.LocalPath);
                var dllDirectory = Path.Combine(
                    baseDirectory,
                    is64
                        ? Path.Combine("librdkafka", "x64")
                        : Path.Combine("librdkafka", "x86"));
                path = Path.Combine(dllDirectory, "librdkafka.dll");

                if (!File.Exists(path))
                {
                    dllDirectory = Path.Combine(
                        baseDirectory,
                        is64
                            ? @"runtimes\win-x64\native"
                            : @"runtimes\win-x86\native");
                    path = Path.Combine(dllDirectory, "librdkafka.dll");
                }

                if (!File.Exists(path))
                {
                    dllDirectory = Path.Combine(
                        baseDirectory,
                        is64 ? "x64" : "x86");
                    path = Path.Combine(dllDirectory, "librdkafka.dll");
                }

                if (!File.Exists(path))
                {
                    path = Path.Combine(baseDirectory, "librdkafka.dll");
                }
            }

            if (WindowsNative.LoadLibraryEx(path, IntPtr.Zero, WindowsNative.LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH) == IntPtr.Zero)
            {
                // catch the last win32 error by default and keep the associated default message
                var win32Exception = new Win32Exception();
                var additionalMessage =
                    $"Error while loading librdkafka.dll or its dependencies from {path}. " +
                    $"Check the directory exists, if not check your deployment process. " +
                    $"You can also load the library and its dependencies by yourself " +
                    $"before any call to Confluent.Kafka";

                throw new InvalidOperationException(additionalMessage, win32Exception);
            }

            BindNativeMethods<DefaultLibRdKafkaBindings>();
        }

#endif

        private static void BindNativeMethods<TBindingsProvider>() where TBindingsProvider : ILibrdkafkaBindings
        {
            if (!TryBindNativeMethods<TBindingsProvider>())
            {
                throw new DllNotFoundException("Failed to load the librdkafka native library.");
            }
        }
        
        private static void BindNativeMethods<TBindingsProvider1, TBindingsProvider2>() 
            where TBindingsProvider1 : ILibrdkafkaBindings 
            where TBindingsProvider2 : ILibrdkafkaBindings
        {
            if (!TryBindNativeMethods<TBindingsProvider1>() && !TryBindNativeMethods<TBindingsProvider2>())
            {
                throw new DllNotFoundException("Failed to load the librdkafka native library.");
            }
        }
        
        private static bool TryBindNativeMethods<TBindingsProvider>() where TBindingsProvider : ILibrdkafkaBindings
        {
            try
            {
                BindLibrdkafkaFrom<TBindingsProvider>();
                // throws if the native library failed to load.
                _rd_kafka_err2str(ErrorCode.NoError);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private static void LoadNetStandardBindings(string userSpecifiedPath)
        {
            if (userSpecifiedPath != null)
            {
                if (WindowsNative.LoadLibraryEx(userSpecifiedPath, IntPtr.Zero, WindowsNative.LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH) == IntPtr.Zero)
                {
                    // TODO: The Win32Exception class is not available in .NET Standard, which is the easy way to get the message string corresponding to
                    // a win32 error. FormatMessage is not straightforward to p/invoke, so leaving this as a job for another day.
                    throw new InvalidOperationException($"Failed to load librdkafka at location '{userSpecifiedPath}'. Win32 error: {Marshal.GetLastWin32Error()}");
                }
            }

            BindNativeMethods<DefaultLibRdKafkaBindings>();
        }

        private static void LoadOSXBindings(string userSpecifiedPath)
        {
            if (userSpecifiedPath != null)
            {
                if (PosixNative.dlopen(userSpecifiedPath, RTLD_NOW) == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to load librdkafka at location '{userSpecifiedPath}'. dlerror: '{PosixNative.LastError}'.");
                }
            }
            
            BindNativeMethods<DefaultLibRdKafkaBindings>();
        }

        private static void LoadLinuxBindings(string userSpecifiedPath)
        {
            if (userSpecifiedPath != null)
            {
                if (PosixNative.dlopen(userSpecifiedPath, RTLD_NOW) == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to load librdkafka at location '{userSpecifiedPath}'. dlerror: '{PosixNative.LastError}'.");
                }

                BindNativeMethods<DefaultLibRdKafkaBindings>();
            }
            else
            {
                var osName = PlatformApis.GetOSName();
                if (osName.Equals("alpine", StringComparison.OrdinalIgnoreCase))
                {
                    BindNativeMethods<AlpineLibRdKafkaBindings>();
                }
                else
                {
                    // Try to load first the shared library with GSSAPI linkage 
                    // and then the one without.
                    BindNativeMethods<DefaultLibRdKafkaBindings, CentOsLibRdKafkaBindings>();
                }
            }
        }

        /// <summary>
        ///  Mimicks what ctor in <see cref="Error"/> will do
        /// </summary>
        private static ErrorCode GetErrorCodeAndDestroy(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return ErrorCode.NoError;
            }

            var code = error_code(ptr);
            error_destroy(ptr);
            return code;
        }
    }
}
