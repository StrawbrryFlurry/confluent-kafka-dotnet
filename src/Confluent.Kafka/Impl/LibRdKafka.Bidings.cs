using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Confluent.Kafka.Admin;
using Confluent.Kafka.Internal;
using DynamicNativeMethodBinding;

namespace Confluent.Kafka.Impl;

#if NET462
[NativeBindingsFrom(Name = "AlpineLibRdKafka", Dll = "alpine-librdkafka.so")]
[NativeBindingsFrom(Name = "CentOsLibRdKafka", Dll = "centos8-librdkafka.so")]
#else
[NativeBindingsFrom(Name = "AlpineLibRdKafka", Dll = "alpine-librdkafka")]
[NativeBindingsFrom(Name = "CentOsLibRdKafka", Dll = "centos8-librdkafka")]
#endif
[NativeBindingsFrom(Name = "DefaultLibRdKafka", Dll = "librdkafka")]
internal static unsafe partial class Librdkafka
{
    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void DeliveryReportDelegate(
        IntPtr rk,
        /* const rd_kafka_message_t * */ IntPtr rkmessage,
        // ref rd_kafka_message rkmessage,
        IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void CommitDelegate(IntPtr rk,
        ErrorCode err,
        /* rd_kafka_topic_partition_list_t * */ IntPtr offsets,
        IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void ErrorDelegate(IntPtr rk,
        ErrorCode err, string reason, IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void RebalanceDelegate(IntPtr rk,
        ErrorCode err,
        /* rd_kafka_topic_partition_list_t * */ IntPtr partitions,
        IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void LogDelegate(IntPtr rk, SyslogLevel level, string fac, string buf);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate int StatsDelegate(IntPtr rk, IntPtr json, UIntPtr json_len, IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate void OAuthBearerTokenRefreshDelegate(IntPtr rk, IntPtr oauthbearer_config, IntPtr opaque);

    [UnmanagedFunctionPointer(callingConvention: CallingConvention.Cdecl)]
    internal delegate int PartitionerDelegate(
        /* const rd_kafka_topic_t * */ IntPtr rkt,
        IntPtr keydata,
        UIntPtr keylen,
        int partition_cnt,
        IntPtr rkt_opaque,
        IntPtr msg_opaque);
    
    [GenerateAccessorMethod(MethodName = "version")]
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_version();

    [GenerateAccessorMethod(MethodName = "version_str")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_version_str();

    [GenerateAccessorMethod(MethodName = "get_debug_contexts")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_get_debug_contexts();

    [GenerateAccessorMethod(MethodName = "err2str")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_err2str(ErrorCode err);

    [GenerateAccessorMethod(MethodName = "last_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_last_error();

    [GenerateAccessorMethod(MethodName = "fatal_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_fatal_error(
        IntPtr rk,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "message_errstr")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_message_errstr(
        /* rd_kafka_message_t * */ IntPtr rkmessage);

    [GenerateAccessorMethod(MethodName = "topic_partition_list_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_topic_partition_list_t * */ IntPtr
        rd_kafka_topic_partition_list_new(IntPtr size);

    [GenerateAccessorMethod(MethodName = "topic_partition_list_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_partition_list_destroy(
        /* rd_kafka_topic_partition_list_t * */ IntPtr rkparlist);

    [GenerateAccessorMethod(MethodName = "topic_partition_list_add")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_topic_partition_t * */ IntPtr
        rd_kafka_topic_partition_list_add(
            /* rd_kafka_topic_partition_list_t * */ IntPtr rktparlist,
            string topic, int partition);

    [GenerateAccessorMethod(MethodName = "headers_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_headers_t * */ IntPtr
        rd_kafka_headers_new(IntPtr size);

    [GenerateAccessorMethod(MethodName = "headers_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_headers_destroy(
        /* rd_kafka_headers_t * */ IntPtr hdrs);

    [GenerateAccessorMethod(MethodName = "headers_add")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_header_add(
        /* rd_kafka_headers_t * */ IntPtr hdrs,
        /* const char * */ IntPtr name,
        /* ssize_t */ IntPtr name_size,
        /* const void * */ IntPtr value,
        /* ssize_t */ IntPtr value_size
    );

    [GenerateAccessorMethod(MethodName = "header_get_all")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_header_get_all(
        /* const rd_kafka_headers_t * */ IntPtr hdrs,
        /* const size_t */ IntPtr idx,
        /* const char ** */ out IntPtr namep,
        /* const void ** */ out IntPtr valuep,
        /* size_t * */ out IntPtr sizep);

    [GenerateAccessorMethod(MethodName = "message_timestamp")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* int64_t */ long rd_kafka_message_timestamp(
        /* rd_kafka_message_t * */ IntPtr rkmessage,
        /* r_kafka_timestamp_type_t * */ out IntPtr tstype);

    [GenerateAccessorMethod(MethodName = "message_headers")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_message_headers(
        /* rd_kafka_message_t * */ IntPtr rkmessage,
        /* r_kafka_headers_t * */ out IntPtr hdrs);

    [GenerateAccessorMethod(MethodName = "message_status")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern PersistenceStatus rd_kafka_message_status(
        /* rd_kafka_message_t * */ IntPtr rkmessage);

    [GenerateAccessorMethod(MethodName = "message_leader_epoch")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_message_leader_epoch(
        /* rd_kafka_message_t * */ IntPtr rkmessage);

    [GenerateAccessorMethod(MethodName = "message_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_message_destroy(
        /* rd_kafka_message_t * */ IntPtr rkmessage);

    [GenerateAccessorMethod(MethodName = "conf_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeConfigHandle rd_kafka_conf_new();

    [GenerateAccessorMethod(MethodName = "conf_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_destroy(IntPtr conf);

    [GenerateAccessorMethod(MethodName = "conf_dup")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_conf_dup(IntPtr conf);

    [GenerateAccessorMethod(MethodName = "default_topic_conf_dup")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeTopicConfigHandle rd_kafka_default_topic_conf_dup(SafeKafkaHandle rk);

    [GenerateAccessorMethod(MethodName = "conf_set")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConfRes rd_kafka_conf_set(
        IntPtr conf,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "conf_set_dr_msg_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_dr_msg_cb(
        IntPtr conf,
        DeliveryReportDelegate dr_msg_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_rebalance_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_rebalance_cb(
        IntPtr conf, RebalanceDelegate rebalance_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_offset_commit_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_offset_commit_cb(
        IntPtr conf, CommitDelegate commit_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_error_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_error_cb(
        IntPtr conf, ErrorDelegate error_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_log_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_log_cb(IntPtr conf, LogDelegate log_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_oauthbearer_token_refresh_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_oauthbearer_token_refresh_cb(IntPtr conf, OAuthBearerTokenRefreshDelegate oauthbearer_token_refresh_cb);

    [GenerateAccessorMethod(MethodName = "oauthbearer_set_token")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_oauthbearer_set_token(
        IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string token_value,
        long md_lifetime_ms,
        [MarshalAs(UnmanagedType.LPStr)] string md_principal_name,
        [MarshalAs(UnmanagedType.LPArray)] string[] extensions, UIntPtr extension_size,
        StringBuilder errstr, UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "oauthbearer_set_token_failure")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_oauthbearer_set_token_failure(
        IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string errstr);

    [GenerateAccessorMethod(MethodName = "conf_set_stats_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_stats_cb(IntPtr conf, StatsDelegate stats_cb);

    [GenerateAccessorMethod(MethodName = "conf_set_default_topic_conf")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_set_default_topic_conf(
        IntPtr conf, IntPtr tconf);

    [GenerateAccessorMethod(MethodName = "conf_get_default_topic_conf")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeTopicConfigHandle rd_kafka_conf_get_default_topic_conf(
        SafeConfigHandle conf);

    [GenerateAccessorMethod(MethodName = "conf_get")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConfRes rd_kafka_conf_get(
        IntPtr conf,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        StringBuilder dest, ref UIntPtr dest_size);

    [GenerateAccessorMethod(MethodName = "topic_conf_get")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConfRes rd_kafka_topic_conf_get(
        IntPtr conf,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        StringBuilder dest, ref UIntPtr dest_size);

    [GenerateAccessorMethod(MethodName = "conf_dump")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* const char ** */ IntPtr rd_kafka_conf_dump(
        IntPtr conf, /* size_t * */ out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "topic_conf_dump")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* const char ** */ IntPtr rd_kafka_topic_conf_dump(
        IntPtr conf, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "conf_dump_free")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_conf_dump_free(/* const char ** */ IntPtr arr, UIntPtr cnt);

    [GenerateAccessorMethod(MethodName = "topic_conf_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeTopicConfigHandle rd_kafka_topic_conf_new();

    [GenerateAccessorMethod(MethodName = "topic_conf_dup")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeTopicConfigHandle rd_kafka_topic_conf_dup(
        SafeTopicConfigHandle conf);

    [GenerateAccessorMethod(MethodName = "topic_conf_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_conf_destroy(IntPtr conf);

    [GenerateAccessorMethod(MethodName = "topic_conf_set")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConfRes rd_kafka_topic_conf_set(
        IntPtr conf,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "topic_conf_set_opaque")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_conf_set_opaque(
        IntPtr topic_conf, IntPtr opaque);

    [GenerateAccessorMethod(MethodName = "topic_conf_set_partitioner_cb")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_conf_set_partitioner_cb(
        IntPtr topic_conf, PartitionerDelegate partitioner_cb);

    [GenerateAccessorMethod(MethodName = "topic_partition_available")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool rd_kafka_topic_partition_available(
        IntPtr rkt, int partition);

    [GenerateAccessorMethod(MethodName = "topic_partition_get_leader_epoch")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_topic_partition_get_leader_epoch(
        IntPtr rkt);

    [GenerateAccessorMethod(MethodName = "topic_partition_set_leader_epoch")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_partition_set_leader_epoch(
        IntPtr rkt, int leader_epoch);

    [GenerateAccessorMethod(MethodName = "init_transactions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_init_transactions(
        IntPtr rk, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "begin_transaction")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_begin_transaction(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "commit_transaction")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_commit_transaction(
        IntPtr rk, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "abort_transaction")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_abort_transaction(
        IntPtr rk, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "send_offsets_to_transaction")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_send_offsets_to_transaction(
        IntPtr rk, IntPtr offsets, IntPtr consumer_group_metadata,
        IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "consumer_group_metadata")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_consumer_group_metadata(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "consumer_group_metadata_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_consumer_group_metadata_destroy(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "consumer_group_metadata_write")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_consumer_group_metadata_write(
        /* rd_kafka_consumer_group_metadata_t * */IntPtr cgmd,
        /* const void ** */ out IntPtr valuep,
        /* size_t * */ out IntPtr sizep);

    [GenerateAccessorMethod(MethodName = "consumer_group_metadata_read")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_consumer_group_metadata_read(
        /* rd_kafka_consumer_group_metadata_t ** */ out IntPtr cgmdp,
        byte[] buffer, IntPtr size);

    [GenerateAccessorMethod(MethodName = "kafka_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeKafkaHandle rd_kafka_new(
        RdKafkaType type, IntPtr conf,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_destroy(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "destroy_flags")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_destroy_flags(IntPtr rk, IntPtr flags);

    [GenerateAccessorMethod(MethodName = "name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* const char * */ IntPtr rd_kafka_name(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "memberid")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* char * */ IntPtr rd_kafka_memberid(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "Uuid_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_Uuid_t * */IntPtr rd_kafka_Uuid_new(
        long most_significant_bits,
        long least_significant_bits
    );

    [GenerateAccessorMethod(MethodName = "Uuid_base64str")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* char * */IntPtr rd_kafka_Uuid_base64str(IntPtr uuid);

    [GenerateAccessorMethod(MethodName = "Uuid_most_significant_bits")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern long rd_kafka_Uuid_most_significant_bits(IntPtr uuid);

    [GenerateAccessorMethod(MethodName = "Uuid_least_significant_bits")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern long rd_kafka_Uuid_least_significant_bits(IntPtr uuid);

    [GenerateAccessorMethod(MethodName = "Uuid_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_Uuid_destroy(IntPtr uuid);

    [GenerateAccessorMethod(MethodName = "topic_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern SafeTopicHandle rd_kafka_topic_new(
        IntPtr rk, IntPtr topic,
        /* rd_kafka_topic_conf_t * */ IntPtr conf);

    [GenerateAccessorMethod(MethodName = "topic_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_topic_destroy(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "topic_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* const char * */ IntPtr rd_kafka_topic_name(IntPtr rkt);

    [GenerateAccessorMethod(MethodName = "poll_set_consumer")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_poll_set_consumer(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "poll")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_poll(IntPtr rk, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "query_watermark_offsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_query_watermark_offsets(IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string topic,
        int partition, out long low, out long high, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "get_watermark_offsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_get_watermark_offsets(IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string topic,
        int partition, out long low, out long high);

    [GenerateAccessorMethod(MethodName = "offsets_for_times")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_offsets_for_times(IntPtr rk,
        /* rd_kafka_topic_partition_list_t * */ IntPtr offsets,
        IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "mem_free")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_mem_free(IntPtr rk, IntPtr ptr);

    [GenerateAccessorMethod(MethodName = "subscribe")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_subscribe(IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr topics);

    [GenerateAccessorMethod(MethodName = "unsubscribe")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_unsubscribe(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "subscription")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_subscription(IntPtr rk,
        /* rd_kafka_topic_partition_list_t ** */ out IntPtr topics);

    [GenerateAccessorMethod(MethodName = "consumer_poll")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_message_t * */ IntPtr rd_kafka_consumer_poll(
        IntPtr rk, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "consumer_close")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_consumer_close(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "assign")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_assign(IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "incremental_assign")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_incremental_assign(IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "incremental_unassign")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_incremental_unassign(IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "assignment_lost")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_assignment_lost(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "rebalance_protocol")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_rebalance_protocol(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "assignment")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_assignment(IntPtr rk,
        /* rd_kafka_topic_partition_list_t ** */ out IntPtr topics);

    [GenerateAccessorMethod(MethodName = "offsets_store")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_offsets_store(
        IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr offsets);

    [GenerateAccessorMethod(MethodName = "commit")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_commit(
        IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr offsets,
        bool async);

    [GenerateAccessorMethod(MethodName = "commit_queue")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_commit_queue(
        IntPtr rk,
        /* const rd_kafka_topic_partition_list_t * */ IntPtr offsets,
        /* rd_kafka_queue_t * */ IntPtr rkqu,
        /* offset_commit_cb * */ CommitDelegate cb,
        /* void * */ IntPtr opaque);

    [GenerateAccessorMethod(MethodName = "pause_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_pause_partitions(
        IntPtr rk, IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "resume_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_resume_partitions(
        IntPtr rk, IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "seek")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_seek(
        IntPtr rkt, int partition, long offset, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "seek_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_seek_partitions(
        IntPtr rkt, IntPtr partitions, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "committed")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_committed(
        IntPtr rk, IntPtr partitions, IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "position")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_position(
        IntPtr rk, IntPtr partitions);

    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_produceva(
        IntPtr rk,
        rd_kafka_vu* vus,
        IntPtr size);
    
    internal static ErrorCode produceva(
        IntPtr rk,
        string topic,
        int partition,
        IntPtr msgflags,
        IntPtr val, UIntPtr len,
        IntPtr key, UIntPtr keylen,
        long timestamp,
        IntPtr headers,
        IntPtr msg_opaque)
    {
        IntPtr topicStrPtr = Marshal.StringToHGlobalAnsi(topic);
            
        try
        {
            rd_kafka_vu* vus = stackalloc rd_kafka_vu[] {
                new rd_kafka_vu() {vt = rd_kafka_vtype.Topic,     data  = new vu_data() {topic = topicStrPtr}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Partition, data  = new vu_data() {partition = partition}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.MsgFlags,  data  = new vu_data() {msgflags = msgflags}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Value,     data  = new vu_data() {val = new ptr_and_size() {ptr = val, size = len}}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Key,       data  = new vu_data() {key = new ptr_and_size() {ptr = key, size = keylen}}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Timestamp, data  = new vu_data() {timestamp = timestamp}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Headers,   data  = new vu_data() {headers = headers}},
                new rd_kafka_vu() {vt = rd_kafka_vtype.Opaque,    data  = new vu_data() {opaque = msg_opaque}},
            };

            IntPtr result = _rd_kafka_produceva(rk, vus, new IntPtr(8));
            return GetErrorCodeAndDestroy(result);
        }
        finally
        {
            Marshal.FreeHGlobal(topicStrPtr);
        }
    }

    [GenerateAccessorMethod(MethodName = "flush")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_flush(
        IntPtr rk,
        IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "metadata")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_metadata(
        IntPtr rk, bool all_topics,
        /* rd_kafka_topic_t * */ IntPtr only_rkt,
        /* const struct rd_kafka_metadata ** */ out IntPtr metadatap,
        IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "metadata_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_metadata_destroy(
        /* const struct rd_kafka_metadata * */ IntPtr metadata);

    [GenerateAccessorMethod(MethodName = "list_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_list_groups(
        IntPtr rk, string group, out IntPtr grplistp,
        IntPtr timeout_ms);

    [GenerateAccessorMethod(MethodName = "group_list_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_group_list_destroy(
        IntPtr grplist);

    [GenerateAccessorMethod(MethodName = "brokers_add")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_brokers_add(IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string brokerlist);

    [GenerateAccessorMethod(MethodName = "sasl_set_credentials")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_sasl_set_credentials(IntPtr rk,
        [MarshalAs(UnmanagedType.LPStr)] string username,
        [MarshalAs(UnmanagedType.LPStr)] string password);

    [GenerateAccessorMethod(MethodName = "outq_len")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_outq_len(IntPtr rk);



    //
    // Admin API
    //

    [GenerateAccessorMethod(MethodName = "AdminOptions_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_new(IntPtr rk, Librdkafka.AdminOp op);

    [GenerateAccessorMethod(MethodName = "AdminOptions_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AdminOptions_destroy(IntPtr options);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_request_timeout")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AdminOptions_set_request_timeout(
        IntPtr options,
        IntPtr timeout_ms,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_operation_timeout")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AdminOptions_set_operation_timeout(
        IntPtr options,
        IntPtr timeout_ms,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_validate_only")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AdminOptions_set_validate_only(
        IntPtr options,
        IntPtr true_or_false,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_incremental")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AdminOptions_set_incremental(
        IntPtr options,
        IntPtr true_or_false,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_broker")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AdminOptions_set_broker(
        IntPtr options,
        int broker_id,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_opaque")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AdminOptions_set_opaque(
        IntPtr options,
        IntPtr opaque);


    [GenerateAccessorMethod(MethodName = "AdminOptions_set_require_stable_offsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_set_require_stable_offsets(
        IntPtr options,
        IntPtr true_or_false);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_include_authorized_operations")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_set_include_authorized_operations(
        IntPtr options,
        IntPtr true_or_false);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_isolation_level")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_set_isolation_level(
        IntPtr options,
        IntPtr isolation_level);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_match_consumer_group_states")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_set_match_consumer_group_states(
        IntPtr options,
        ConsumerGroupState[] states,
        UIntPtr statesCnt);

    [GenerateAccessorMethod(MethodName = "AdminOptions_set_match_consumer_group_types")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AdminOptions_set_match_consumer_group_types(
        IntPtr options,
        ConsumerGroupType[] types,
        UIntPtr typesCnt);

    [GenerateAccessorMethod(MethodName = "NewTopic_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_NewTopic_new(
        [MarshalAs(UnmanagedType.LPStr)] string topic,
        IntPtr num_partitions,
        IntPtr replication_factor,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "NewTopic_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_NewTopic_destroy(
        IntPtr new_topic);

    [GenerateAccessorMethod(MethodName = "NewTopic_set_replica_assignment")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_NewTopic_set_replica_assignment(
        IntPtr new_topic,
        int partition,
        int[] broker_ids,
        UIntPtr broker_id_cnt,
        StringBuilder errstr,
        UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "NewTopic_set_config")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_NewTopic_set_config(
        IntPtr new_topic,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);


    [GenerateAccessorMethod(MethodName = "CreateTopics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_CreateTopics(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_NewTopic_t ** */ IntPtr[] new_topics,
        UIntPtr new_topic_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "CreateTopics_result_topics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_CreateTopics_result_topics(
        /* rd_kafka_CreateTopics_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp
    );


    [GenerateAccessorMethod(MethodName = "DeleteTopic_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_DeleteTopic_t * */ IntPtr rd_kafka_DeleteTopic_new(
        [MarshalAs(UnmanagedType.LPStr)] string topic
    );

    [GenerateAccessorMethod(MethodName = "DeleteTopic_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteTopic_destroy(
        /* rd_kafka_DeleteTopic_t * */ IntPtr del_topic);

    [GenerateAccessorMethod(MethodName = "DeleteTopics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteTopics(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_DeleteTopic_t ** */ IntPtr[] del_topics,
        UIntPtr del_topic_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DeleteTopics_result_topics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteTopics_result_topics(
        /* rd_kafka_DeleteTopics_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp
    );


    [GenerateAccessorMethod(MethodName = "DeleteGroup_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_DeleteGroup_t * */ IntPtr rd_kafka_DeleteGroup_new(
        [MarshalAs(UnmanagedType.LPStr)] string group
    );

    [GenerateAccessorMethod(MethodName = "DeleteGroup_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteGroup_destroy(
        /* rd_kafka_DeleteGroup_t * */ IntPtr del_group);

    [GenerateAccessorMethod(MethodName = "DeleteGroups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteGroups(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_DeleteGroup_t ** */ IntPtr[] del_groups,
        UIntPtr del_group_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DeleteGroups_result_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteGroups_result_groups(
        /* rd_kafka_DeleteGroups_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp
    );


    [GenerateAccessorMethod(MethodName = "DeleteRecords_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_DeleteRecords_t * */ IntPtr rd_kafka_DeleteRecords_new(
        /* rd_kafka_topic_partition_list_t * */ IntPtr offsets
    );

    [GenerateAccessorMethod(MethodName = "DeleteRecords_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteRecords_destroy(
        /* rd_kafka_DeleteRecords_t * */ IntPtr del_topic);

    [GenerateAccessorMethod(MethodName = "DeleteRecords")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteRecords(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_DeleteRecords_t ** */ IntPtr[] del_records,
        UIntPtr del_records_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DeleteRecords_result_offsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_topic_partition_list_t * */ IntPtr rd_kafka_DeleteRecords_result_offsets(
        /* rd_kafka_DeleteRecords_result_t * */ IntPtr result);


    [GenerateAccessorMethod(MethodName = "NewPartitions_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_NewPartitions_new(
        [MarshalAs(UnmanagedType.LPStr)] string topic,
        UIntPtr new_total_cnt,
        StringBuilder errstr, UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "NewPartitions_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_NewPartitions_destroy(
        /* rd_kafka_NewPartitions_t * */ IntPtr new_parts);


    [GenerateAccessorMethod(MethodName = "NewPartitions_set_replica_assignment")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_NewPartitions_set_replica_assignment(
        /* rd_kafka_NewPartitions_t * */ IntPtr new_parts,
        int new_partition_idx,
        int[] broker_ids,
        UIntPtr broker_id_cnt,
        StringBuilder errstr, // TODO: Use string[] or char*
        UIntPtr errstr_size);


    [GenerateAccessorMethod(MethodName = "CreatePartitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_CreatePartitions(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_NewPartitions_t ***/ IntPtr[] new_parts,
        UIntPtr new_parts_cnt,
        /* const rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "CreatePartitions_result_topics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_topic_result_t ** */ IntPtr rd_kafka_CreatePartitions_result_topics(
        /* const rd_kafka_CreatePartitions_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp);


    [GenerateAccessorMethod(MethodName = "ConfigSource_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigSource_name(
        ConfigSource configsource);


    [GenerateAccessorMethod(MethodName = "ConfigEntry_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_name(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_value")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_value (
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_source")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConfigSource rd_kafka_ConfigEntry_source(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_is_read_only")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_is_read_only(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_is_default")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_is_default(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_is_sensitive")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_is_sensitive(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_is_synonym")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigEntry_is_synonym (
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry);

    [GenerateAccessorMethod(MethodName = "ConfigEntry_synonyms")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigEntry_t ** */ IntPtr rd_kafka_ConfigEntry_synonyms(
        /* rd_kafka_ConfigEntry_t * */ IntPtr entry,
        /* size_t * */ out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "ResourceType_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ResourceType_name(
        ResourceType restype);

    [GenerateAccessorMethod(MethodName = "ConfigResource_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigResource_t * */ IntPtr rd_kafka_ConfigResource_new(
        ResourceType restype,
        [MarshalAs(UnmanagedType.LPStr)] string resname); // todo: string?

    [GenerateAccessorMethod(MethodName = "ConfigResource_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ConfigResource_destroy(
        /* rd_kafka_ConfigResource_t * */ IntPtr config);

    [GenerateAccessorMethod(MethodName = "ConfigResource_add_config")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_ConfigResource_add_config(
        /* rd_kafka_ConfigResource_t * */ IntPtr config,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    [GenerateAccessorMethod(MethodName = "ConfigResource_set_config")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_ConfigResource_set_config(
        /* rd_kafka_ConfigResource_t * */ IntPtr config,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    [GenerateAccessorMethod(MethodName = "ConfigResource_delete_config")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_ConfigResource_delete_config(
        /* rd_kafka_ConfigResource_t * */ IntPtr config,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    [GenerateAccessorMethod(MethodName = "ConfigResource_add_incremental_config")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_error_t * */ IntPtr rd_kafka_ConfigResource_add_incremental_config(
        /* rd_kafka_ConfigResource_t * */ IntPtr config,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        /* rd_kafka_AlterConfigOpType_t */ AlterConfigOpType optype,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    [GenerateAccessorMethod(MethodName = "ConfigResource_configs")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigEntry_t ** */ IntPtr rd_kafka_ConfigResource_configs(
        /* rd_kafka_ConfigResource_t * */ IntPtr config,
        /* size_t * */ out UIntPtr cntp);


    [GenerateAccessorMethod(MethodName = "ConfigResource_type")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ResourceType rd_kafka_ConfigResource_type(
        /* rd_kafka_ConfigResource_t * */ IntPtr config);

    [GenerateAccessorMethod(MethodName = "ConfigResource_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* char * */ IntPtr rd_kafka_ConfigResource_name(
        /* rd_kafka_ConfigResource_t * */ IntPtr config);

    [GenerateAccessorMethod(MethodName = "ConfigResource_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_ConfigResource_error(
        /* rd_kafka_ConfigResource_t * */ IntPtr config);

    [GenerateAccessorMethod(MethodName = "ConfigResource_error_string")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConfigResource_error_string(
        /* rd_kafka_ConfigResource_t * */ IntPtr config);


    [GenerateAccessorMethod(MethodName = "AlterConfigs")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AlterConfigs (
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_ConfigResource_t ** */ IntPtr[] configs,
        UIntPtr config_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "AlterConfigs_result_resources")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigResource_t ** */ IntPtr rd_kafka_AlterConfigs_result_resources(
        /* rd_kafka_AlterConfigs_result_t * */ IntPtr result,
        out UIntPtr cntp);
    
    [GenerateAccessorMethod(MethodName = "IncrementalAlterConfigs")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_IncrementalAlterConfigs(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_ConfigResource_t ** */ IntPtr[] configs,
        UIntPtr config_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);
    
    [GenerateAccessorMethod(MethodName = "IncrementalAlterConfigs_result_resources")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigResource_t ** */ IntPtr rd_kafka_IncrementalAlterConfigs_result_resources(
        /* rd_kafka_IncrementalAlterConfigs_result_t * */ IntPtr result,
        out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DescribeConfigs")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeConfigs(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_ConfigResource_t ***/ IntPtr[] configs,
        UIntPtr config_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DescribeConfigs_result_resources")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_ConfigResource_t ** */ IntPtr rd_kafka_DescribeConfigs_result_resources(
        /* rd_kafka_DescribeConfigs_result_t * */ IntPtr result,
        out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "AclBinding_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AclBinding_new(
        /* rd_kafka_ResourceType_t */ ResourceType restype,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string name,
        /* rd_kafka_ResourcePatternType_t */ ResourcePatternType resource_pattern_type,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string principal,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string host,
        /* rd_kafka_AclOperation_t */ AclOperation operation,
        /* rd_kafka_AclPermissionType_t */ AclPermissionType permission_type,
        /* char * */ StringBuilder errstr,
        /* size_t */ UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AclBindingFilter_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AclBindingFilter_new(
        /* rd_kafka_ResourceType_t */ ResourceType restype,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string name,
        /* rd_kafka_ResourcePatternType_t */ ResourcePatternType resource_pattern_type,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string principal,
        /* const char * */[MarshalAs(UnmanagedType.LPStr)] string host,
        /* rd_kafka_AclOperation_t */ AclOperation operation,
        /* rd_kafka_AclPermissionType_t */ AclPermissionType permission_type,
        /* char * */ StringBuilder errstr,
        /* size_t */ UIntPtr errstr_size);

    [GenerateAccessorMethod(MethodName = "AclBinding_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AclBinding_destroy(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_restype")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ResourceType rd_kafka_AclBinding_restype(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AclBinding_name(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_resource_pattern_type")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ResourcePatternType rd_kafka_AclBinding_resource_pattern_type(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_principal")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AclBinding_principal(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_host")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AclBinding_host(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_operation")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern AclOperation rd_kafka_AclBinding_operation(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "AclBinding_permission_type")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern AclPermissionType rd_kafka_AclBinding_permission_type(
        /* rd_kafka_AclBinding_t * */ IntPtr acl_binding);

    [GenerateAccessorMethod(MethodName = "CreateAcls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_CreateAcls(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_AclBinding_t ** */ IntPtr[] new_acls,
        UIntPtr new_acls_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "CreateAcls_result_acls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_CreateAcls_result_acls(
        /* const rd_kafka_CreateAcls_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "acl_result_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_acl_result_error(
        /* const rd_kafka_acl_result_t * */ IntPtr aclres);

    [GenerateAccessorMethod(MethodName = "DescribeAcls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeAcls(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_AclBindingFilter_t * */ IntPtr acl_filter,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DescribeAcls_result_acls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeAcls_result_acls(
        /* const rd_kafka_DescribeAcls_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DeleteAcls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteAcls(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_AclBindingFilter_t ** */ IntPtr[] del_acls,
        UIntPtr del_acls_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DeleteAcls_result_responses")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteAcls_result_responses(
        /* rd_kafka_DeleteAcls_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DeleteAcls_result_response_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteAcls_result_response_error(
        /* rd_kafka_DeleteAcls_result_response_t * */ IntPtr result_response);

    [GenerateAccessorMethod(MethodName = "DeleteAcls_result_response_matching_acls")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteAcls_result_response_matching_acls(
        /* rd_kafka_DeleteAcls_result_response_t * */ IntPtr result_response,
        /* size_t * */ out UIntPtr matching_acls_cntp);

    [GenerateAccessorMethod(MethodName = "DeleteConsumerGroupOffsets_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern /* rd_kafka_DeleteConsumerGroupOffsets_t */ IntPtr rd_kafka_DeleteConsumerGroupOffsets_new(
        [MarshalAs(UnmanagedType.LPStr)] string group,
        /* rd_kafka_topic_partition_list_t * */ IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "DeleteConsumerGroupOffsets_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteConsumerGroupOffsets_destroy(
        /* rd_kafka_DeleteConsumerGroupOffsets_t * */ IntPtr del_grp_offsets);

    [GenerateAccessorMethod(MethodName = "DeleteConsumerGroupOffsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DeleteConsumerGroupOffsets(
        /* rd_kafka_t * */ IntPtr rk,
        /* rd_kafka_DeleteConsumerGroupOffsets_t ** */ IntPtr[] del_grp_offsets,
        UIntPtr del_grp_offsets_cnt,
        /* rd_kafka_AdminOptions_t * */ IntPtr options,
        /* rd_kafka_queue_t * */ IntPtr rkqu);

    [GenerateAccessorMethod(MethodName = "DeleteConsumerGroupOffsets_result_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DeleteConsumerGroupOffsets_result_groups(
        /* rd_kafka_DeleteConsumerGroupOffsets_result_t * */ IntPtr result,
        /* size_t * */ out UIntPtr cntp
    );

    [GenerateAccessorMethod(MethodName = "ListConsumerGroupOffsets_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListConsumerGroupOffsets_new(
        [MarshalAs(UnmanagedType.LPStr)] string group, IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroupOffsets_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ListConsumerGroupOffsets_destroy(IntPtr groupPartitions);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroupOffsets_result_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListConsumerGroupOffsets_result_groups(
        IntPtr resultResponse, out UIntPtr groupsTopicPartitionsCount);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroupOffsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ListConsumerGroupOffsets(
        IntPtr handle,
        IntPtr[] listGroupsPartitions,
        UIntPtr listGroupsPartitionsSize,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "AlterConsumerGroupOffsets_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AlterConsumerGroupOffsets_new(
        [MarshalAs(UnmanagedType.LPStr)] string group, IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "AlterConsumerGroupOffsets_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AlterConsumerGroupOffsets_destroy(IntPtr groupPartitions);

    [GenerateAccessorMethod(MethodName = "AlterConsumerGroupOffsets_result_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AlterConsumerGroupOffsets_result_groups(
        IntPtr resultResponse, out UIntPtr groupsTopicPartitionsCount);

    [GenerateAccessorMethod(MethodName = "AlterConsumerGroupOffsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_AlterConsumerGroupOffsets(
        IntPtr handle,
        IntPtr[] alterGroupsPartitions,
        UIntPtr alterGroupsPartitionsSize,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ListConsumerGroups(
        IntPtr handle,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupListing_group_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupListing_group_id(IntPtr grplist);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupListing_is_simple_consumer_group")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupListing_is_simple_consumer_group(IntPtr grplist);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupListing_state")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConsumerGroupState rd_kafka_ConsumerGroupListing_state(IntPtr grplist);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupListing_type")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConsumerGroupType rd_kafka_ConsumerGroupListing_type(IntPtr grplist);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroups_result_valid")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListConsumerGroups_result_valid(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "ListConsumerGroups_result_errors")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListConsumerGroups_result_errors(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DescribeConsumerGroups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeConsumerGroups(
        IntPtr handle,
        [MarshalAs(UnmanagedType.LPArray)] string[] groups,
        UIntPtr groupsCnt,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "DescribeConsumerGroups_result_groups")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeConsumerGroups_result_groups(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_group_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_group_id(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_error(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_is_simple_consumer_group")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_ConsumerGroupDescription_is_simple_consumer_group(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_partition_assignor")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_partition_assignor(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_state")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ConsumerGroupState rd_kafka_ConsumerGroupDescription_state(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_coordinator")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_coordinator(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_authorized_operations")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_authorized_operations(IntPtr grpdesc, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_member_count")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_member_count(IntPtr grpdesc);

    [GenerateAccessorMethod(MethodName = "ConsumerGroupDescription_member")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ConsumerGroupDescription_member(IntPtr grpdesc, IntPtr idx);

    [GenerateAccessorMethod(MethodName = "MemberDescription_client_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberDescription_client_id(IntPtr member);

    [GenerateAccessorMethod(MethodName = "MemberDescription_group_instance_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberDescription_group_instance_id(IntPtr member);

    [GenerateAccessorMethod(MethodName = "MemberDescription_consumer_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberDescription_consumer_id(IntPtr member);

    [GenerateAccessorMethod(MethodName = "MemberDescription_host")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberDescription_host(IntPtr member);

    [GenerateAccessorMethod(MethodName = "MemberDescription_assignment")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberDescription_assignment(IntPtr member);

    [GenerateAccessorMethod(MethodName = "MemberAssignment_topic_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_MemberAssignment_partitions(IntPtr assignment);

    [GenerateAccessorMethod(MethodName = "Node_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_Node_id(IntPtr node);

    [GenerateAccessorMethod(MethodName = "Node_host")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_Node_host(IntPtr node);

    [GenerateAccessorMethod(MethodName = "Node_port")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_Node_port(IntPtr node);

    [GenerateAccessorMethod(MethodName = "Node_rack")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_Node_rack(IntPtr node);

    [GenerateAccessorMethod(MethodName = "topic_result_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_topic_result_error(IntPtr topicres);

    [GenerateAccessorMethod(MethodName = "topic_result_error_string")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_topic_result_error_string(IntPtr topicres);

    [GenerateAccessorMethod(MethodName = "topic_result_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_topic_result_name(IntPtr topicres);


    [GenerateAccessorMethod(MethodName = "group_result_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_group_result_name(IntPtr groupres);

    [GenerateAccessorMethod(MethodName = "group_result_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_group_result_error(IntPtr groupres);

    [GenerateAccessorMethod(MethodName = "group_result_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_group_result_partitions(IntPtr groupres);

    [GenerateAccessorMethod(MethodName = "DescribeUserScramCredentials")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeUserScramCredentials(
        IntPtr handle,
        [MarshalAs(UnmanagedType.LPArray)] string[] users,
        UIntPtr usersCnt,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);
    
    [GenerateAccessorMethod(MethodName = "AlterUserScramCredentials")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_AlterUserScramCredentials(
        IntPtr handle,
        IntPtr[] alterations,
        UIntPtr alterationsCnt,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);
    
    [GenerateAccessorMethod(MethodName = "UserScramCredentialDeletion_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_UserScramCredentialDeletion_new(
        string user,
        ScramMechanism mechanism);

    [GenerateAccessorMethod(MethodName = "UserScramCredentialUpsertion_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_UserScramCredentialUpsertion_new(
        string user,
        ScramMechanism mechanism,
        int iterations,
        byte[] password,
        IntPtr passwordSize,
        byte[] salt,
        IntPtr saltSize);

    [GenerateAccessorMethod(MethodName = "UserScramCredentialAlteration_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_UserScramCredentialAlteration_destroy(
        IntPtr alteration);
    
    [GenerateAccessorMethod(MethodName = "DescribeUserScramCredentials_result_descriptions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeUserScramCredentials_result_descriptions(
        IntPtr event_result,
        out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "UserScramCredentialsDescription_user")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_UserScramCredentialsDescription_user(IntPtr description);


    [GenerateAccessorMethod(MethodName = "UserScramCredentialsDescription_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_UserScramCredentialsDescription_error(IntPtr description);

    [GenerateAccessorMethod(MethodName = "UserScramCredentialsDescription_scramcredentialinfo_count")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_UserScramCredentialsDescription_scramcredentialinfo_count(IntPtr description);
    
    [GenerateAccessorMethod(MethodName = "UserScramCredentialsDescription_scramcredentialinfo")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_UserScramCredentialsDescription_scramcredentialinfo(IntPtr description, int i);
    
    [GenerateAccessorMethod(MethodName = "ScramCredentialInfo_mechanism")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ScramMechanism rd_kafka_ScramCredentialInfo_mechanism(IntPtr scramcredentialinfo);
    
    [GenerateAccessorMethod(MethodName = "ScramCredentialInfo_iterations")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_ScramCredentialInfo_iterations(IntPtr scramcredentialinfo);
    
    [GenerateAccessorMethod(MethodName = "AlterUserScramCredentials_result_responses")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AlterUserScramCredentials_result_responses(
        IntPtr event_result,
        out UIntPtr cntp);
    
    [GenerateAccessorMethod(MethodName = "AlterUserScramCredentials_result_response_user")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AlterUserScramCredentials_result_response_user(IntPtr element);
    
    [GenerateAccessorMethod(MethodName = "AlterUserScramCredentials_result_response_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_AlterUserScramCredentials_result_response_error(IntPtr element);

    [GenerateAccessorMethod(MethodName = "ListOffsets")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ListOffsets(IntPtr handle, IntPtr topic_partition_list, IntPtr options, IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "ListOffsets_result_infos")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListOffsets_result_infos(IntPtr resultPtr, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "ListOffsetsResultInfo_timestamp")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern long rd_kafka_ListOffsetsResultInfo_timestamp(IntPtr element);

    [GenerateAccessorMethod(MethodName = "ListOffsetsResultInfo_topic_partition")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ListOffsetsResultInfo_topic_partition(IntPtr element);

    [GenerateAccessorMethod(MethodName = "DescribeTopics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeTopics(
        IntPtr handle,
        IntPtr topicCollection,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);
    
    [GenerateAccessorMethod(MethodName = "DescribeTopics_result_topics")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeTopics_result_topics(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "TopicCollection_of_topic_names")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicCollection_of_topic_names([MarshalAs(UnmanagedType.LPArray)] string[] topics,
        UIntPtr topicsCnt);
    
    [GenerateAccessorMethod(MethodName = "TopicCollection_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_TopicCollection_destroy(IntPtr topic_collection);

    [GenerateAccessorMethod(MethodName = "TopicDescription_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_error(IntPtr topicdesc);

    [GenerateAccessorMethod(MethodName = "TopicDescription_name")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_name(IntPtr topicdesc);

    [GenerateAccessorMethod(MethodName = "TopicDescription_topic_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_topic_id(IntPtr topicdesc);

    [GenerateAccessorMethod(MethodName = "TopicDescription_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_partitions(IntPtr topicdesc, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "TopicDescription_is_internal")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_is_internal(IntPtr topicdesc);

    [GenerateAccessorMethod(MethodName = "TopicDescription_authorized_operations")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicDescription_authorized_operations(IntPtr topicdesc, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "TopicPartitionInfo_isr")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicPartitionInfo_isr(IntPtr topic_partition_info, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "TopicPartitionInfo_leader")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicPartitionInfo_leader(IntPtr topic_partition_info);
    
    [GenerateAccessorMethod(MethodName = "TopicPartitionInfo_partition")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern int rd_kafka_TopicPartitionInfo_partition(IntPtr topic_partition_info);

    [GenerateAccessorMethod(MethodName = "TopicPartitionInfo_replicas")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_TopicPartitionInfo_replicas(IntPtr topic_partition_info, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DescribeCluster")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_DescribeCluster(
        IntPtr handle,
        IntPtr optionsPtr,
        IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "DescribeCluster_result_nodes")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeCluster_result_nodes(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DescribeCluster_result_authorized_operations")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeCluster_result_authorized_operations(IntPtr result, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "DescribeCluster_result_controller")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeCluster_result_controller(IntPtr result);

    [GenerateAccessorMethod(MethodName = "DescribeCluster_result_cluster_id")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_DescribeCluster_result_cluster_id(IntPtr result);

    [GenerateAccessorMethod(MethodName = "ElectLeadersRequest_New")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ElectLeaders_new(ElectionType electionType, IntPtr partitions);

    [GenerateAccessorMethod(MethodName = "ElectLeadersRequest_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ElectLeaders_destroy(IntPtr electLeader);

    [GenerateAccessorMethod(MethodName = "ElectLeaders")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_ElectLeaders(IntPtr handle, IntPtr electLeaderRequest, IntPtr options, IntPtr resultQueuePtr);

    [GenerateAccessorMethod(MethodName = "ElectLeaders_result_partitions")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_ElectLeaders_result_partitions(IntPtr result_event, out UIntPtr cntp);

    [GenerateAccessorMethod(MethodName = "TopicPartitionResult_partition")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_topic_partition_result_partition(IntPtr result);

    [GenerateAccessorMethod(MethodName = "TopicPartitionResult_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_topic_partition_result_error(IntPtr result);

    //
    // Queues
    //

    [GenerateAccessorMethod(MethodName = "queue_new")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_queue_new(IntPtr rk);

    [GenerateAccessorMethod(MethodName = "queue_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_queue_destroy(IntPtr rkqu);

    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_queue_poll(IntPtr rkqu, IntPtr timeout_ms);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IntPtr queue_poll(IntPtr rkqu, int timeout_ms)
        => _rd_kafka_queue_poll(rkqu, (IntPtr)timeout_ms);

    //
    // Events
    //

    [GenerateAccessorMethod(MethodName = "event_destroy")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_event_destroy(IntPtr rkev);

    [GenerateAccessorMethod(MethodName = " event_type")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern Librdkafka.EventType rd_kafka_event_type(IntPtr rkev);

    [GenerateAccessorMethod(MethodName = "event_opaque")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_event_opaque(IntPtr rkev);

    [GenerateAccessorMethod(MethodName = "event_error")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_event_error(IntPtr rkev);

    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_event_error_string(IntPtr rkev);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string event_error_string(IntPtr rkev)
        => Util.Marshal.PtrToStringUTF8(_rd_kafka_event_error_string(rkev));

    [GenerateAccessorMethod(MethodName = "event_topic_partition_list")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_event_topic_partition_list(IntPtr rkev);


    //
    // error_t
    //

    [GenerateAccessorMethod(MethodName = "error_code")]    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern ErrorCode rd_kafka_error_code(IntPtr error);

    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_error_string(IntPtr error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string error_string(IntPtr error)
        => Util.Marshal.PtrToStringUTF8(_rd_kafka_error_string(error));
    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_error_is_fatal(IntPtr error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool error_is_fatal(IntPtr error) 
        => _rd_kafka_error_is_fatal(error) != IntPtr.Zero;
    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_error_is_retriable(IntPtr error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool error_is_retriable(IntPtr error)
        => _rd_kafka_error_is_retriable(error) != IntPtr.Zero;
    
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr rd_kafka_error_txn_requires_abort(IntPtr error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool error_txn_requires_abort(IntPtr error) 
        => _rd_kafka_error_txn_requires_abort(error) != IntPtr.Zero;
    
    [GenerateAccessorMethod(MethodName = "error_destroy")]
    [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
    internal static extern void rd_kafka_error_destroy(IntPtr error);
}