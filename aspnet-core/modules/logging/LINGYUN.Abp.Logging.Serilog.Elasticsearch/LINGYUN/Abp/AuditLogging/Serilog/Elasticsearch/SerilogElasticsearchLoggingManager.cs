﻿using LINGYUN.Abp.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace LINGYUN.Abp.Logging.Serilog.Elasticsearch
{
    [Dependency(ReplaceServices = true)]
    public class SerilogElasticsearchLoggingManager : ILoggingManager, ISingletonDependency
    {
        private static readonly Regex IndexFormatRegex = new Regex(@"^(.*)(?:\{0\:.+\})(.*)$");

        private readonly IObjectMapper _objectMapper;
        private readonly AbpLoggingSerilogElasticsearchOptions _options;
        private readonly IElasticsearchClientFactory _clientFactory;

        public ILogger<SerilogElasticsearchLoggingManager> Logger { protected get; set; }

        public SerilogElasticsearchLoggingManager(
            IObjectMapper objectMapper,
            IOptions<AbpLoggingSerilogElasticsearchOptions> options,
            IElasticsearchClientFactory clientFactory)
        {
            _objectMapper = objectMapper;
            _clientFactory = clientFactory;
            _options = options.Value;

            Logger = NullLogger<SerilogElasticsearchLoggingManager>.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">时间类型或者转换为timestamp都可以查询</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<LogInfo> GetAsync(
            string id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = _clientFactory.Create();

            var response = await client.SearchAsync<SerilogInfo>(
                dsl =>
                    dsl.Index(CreateIndex())
                       .Query(
                            (q) => q.Bool(
                                (b) => b.Should(
                                    (s) => s.Term(
                                        (t) => t.Field("@timestamp").Value(id))))),
                cancellationToken);

            return _objectMapper.Map<SerilogInfo, LogInfo>(response.Documents.FirstOrDefault());
        }

        public virtual async Task<long> GetCountAsync(
            DateTime? startTime = null,
            DateTime? endTime = null,
            string context = null,
            string requestId = null,
            string requestPath = null,
            string correlationId = null,
            int? processId = null,
            int? threadId = null,
            bool? hasException = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = _clientFactory.Create();

            var querys = BuildQueryDescriptor(
                startTime,
                endTime,
                context,
                requestId,
                requestPath,
                correlationId,
                processId,
                threadId,
                hasException);

            var response = await client.CountAsync<SerilogInfo>((dsl) => 
                dsl.Index(CreateIndex())
                   .Query(log => log.Bool(b => b.Must(querys.ToArray()))),
                cancellationToken);

            return response.Count;
        }

        /// <summary>
        /// 获取日志列表
        /// </summary>
        /// <param name="sorting">排序字段，注意：忽略排序字段仅使用timestamp排序，根据传递的ASC、DESC字段区分倒序还是正序</param>
        /// <param name="maxResultCount"></param>
        /// <param name="skipCount"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="context"></param>
        /// <param name="requestId"></param>
        /// <param name="requestPath"></param>
        /// <param name="correlationId"></param>
        /// <param name="processId"></param>
        /// <param name="threadId"></param>
        /// <param name="hasException"></param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<List<LogInfo>> GetListAsync(
            string sorting = null,
            int maxResultCount = 50,
            int skipCount = 0,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string context = null,
            string requestId = null,
            string requestPath = null,
            string correlationId = null,
            int? processId = null,
            int? threadId = null,
            bool? hasException = null,
            bool includeDetails = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = _clientFactory.Create();

            var sortOrder = !sorting.IsNullOrWhiteSpace() && sorting.EndsWith("desc", StringComparison.InvariantCultureIgnoreCase)
                ? SortOrder.Descending : SortOrder.Ascending;

            var querys = BuildQueryDescriptor(
                startTime,
                endTime,
                context,
                requestId,
                requestPath,
                correlationId,
                processId,
                threadId,
                hasException);

            SourceFilterDescriptor<SerilogInfo> ConvertFileSystem(SourceFilterDescriptor<SerilogInfo> selector)
            {
                selector.IncludeAll();
                if (!includeDetails)
                {
                    selector.Excludes(field =>
                        field.Field("exceptions"));
                }

                return selector;
            }

            var response = await client.SearchAsync<SerilogInfo>((dsl) =>
                dsl.Index(CreateIndex())
                   .Query(log => log.Bool(b => b.Must(querys.ToArray())))
                   .Source(ConvertFileSystem)
                   .Sort(log => log.Field("@timestamp", sortOrder))
                   .From(skipCount)
                   .Size(maxResultCount),
                cancellationToken);

            return _objectMapper.Map<List<SerilogInfo>, List<LogInfo>>(response.Documents.ToList());
        }

        protected virtual List<Func<QueryContainerDescriptor<SerilogInfo>, QueryContainer>> BuildQueryDescriptor(
            DateTime? startTime = null,
            DateTime? endTime = null,
            string context = null,
            string requestId = null,
            string requestPath = null,
            string correlationId = null,
            int? processId = null,
            int? threadId = null,
            bool? hasException = null)
        {
            var querys = new List<Func<QueryContainerDescriptor<SerilogInfo>, QueryContainer>>();

            if (startTime.HasValue)
            {
                querys.Add((log) => log.DateRange((q) => q.Field(f => f.TimeStamp).GreaterThanOrEquals(startTime)));
            }
            if (endTime.HasValue)
            {
                querys.Add((log) => log.DateRange((q) => q.Field(f => f.TimeStamp).LessThanOrEquals(endTime)));
            }
            if (!context.IsNullOrWhiteSpace())
            {
                querys.Add((log) => log.Term((q) => q.Field((f) => f.Fields.Context.Suffix("keyword")).Value(context)));
            }
            if (!requestId.IsNullOrWhiteSpace())
            {
                querys.Add((log) => log.Match((q) => q.Field(f => f.Fields.RequestId.Suffix("keyword")).Query(requestId)));
            }
            if (!requestPath.IsNullOrWhiteSpace())
            {
                querys.Add((log) => log.Term((q) => q.Field(f => f.Fields.RequestPath.Suffix("keyword")).Value(requestPath)));
            }
            if (!correlationId.IsNullOrWhiteSpace())
            {
                querys.Add((log) => log.Term((q) => q.Field(f => f.Fields.CorrelationId.Suffix("keyword")).Value(correlationId)));
            }
            if (processId.HasValue)
            {
                querys.Add((log) => log.Term((q) => q.Field(f => f.Fields.ProcessId).Value(processId)));
            }
            if (threadId.HasValue)
            {
                querys.Add((log) => log.Term((q) => q.Field(f => f.Fields.ThreadId).Value(threadId)));
            }

            if (hasException.HasValue)
            {
                if (hasException.Value)
                {
                    /*  存在exceptions字段则就是有异常信息
                     * "exists": {
                            "field": "exceptions"
                        }
                     */
                    querys.Add(
                        (q) => q.Exists(
                            (e) => e.Field("exceptions")));
                }
                else
                {
                    // 不存在 exceptions字段就是没有异常信息的消息
                    /*
                     * "bool": {
                            "must_not": [
                                {
                                    "exists": {
                                        "field": "exceptions"
                                    }
                                }
                            ]
                        }
                     */
                    querys.Add(
                        (q) => q.Bool(
                            (b) => b.MustNot(
                                (m) => m.Exists(
                                    (e) => e.Field("exceptions")))));
                }
            }

            return querys;
        }

        protected virtual string CreateIndex(DateTimeOffset? offset = null)
        {
            if (!offset.HasValue)
            {
                return IndexFormatRegex.Replace(_options.IndexFormat, @"$1*$2");
            }
            return string.Format(_options.IndexFormat, offset.Value).ToLowerInvariant();
        }
    }
}
