﻿namespace LINGYUN.Abp.Logging.Serilog.Elasticsearch
{
    public class SerilogField
    {
        [Nest.PropertyName("SourceContext")]
        public string Context { get; set; }

        [Nest.PropertyName("ActionId")]
        public string ActionId { get; set; }

        [Nest.PropertyName("ActionName")]
        public string ActionName { get; set; }

        [Nest.PropertyName("RequestId")]
        public string RequestId { get; set; }

        [Nest.PropertyName("RequestPath")]
        public string RequestPath { get; set; }

        [Nest.PropertyName("ConnectionId")]
        public string ConnectionId { get; set; }

        [Nest.PropertyName("CorrelationId")]
        public string CorrelationId { get; set; }

        [Nest.PropertyName("ClientId")]
        public string ClientId { get; set; }

        [Nest.PropertyName("UserId")]
        public string UserId { get; set; }

        [Nest.PropertyName("ProcessId")]
        public int ProcessId { get; set; }

        [Nest.PropertyName("ThreadId")]
        public int ThreadId { get; set; }
    }
}
