using System;
using System.Collections.Generic;
using System.Net;

namespace JSIStudios.SimpleRESTServices.Client
{
    public class RequestSettings
    {
        public virtual string ContentType { get; set; }
        public int RetryCount { get; set; }
        public int RetryDelayInMS { get; set; }
        public IEnumerable<int> Non200SuccessCodes { get; set; }
        public virtual string Accept { get; set; }
        public Dictionary<int, Action<Response>> ResponseActions { get; set; }
        public string UserAgent { get; set; }
        public ICredentials Credecials { get; set; }
        public int Timeout { get; set; }
        public bool ChunkRequest { get; set; }
        public Dictionary<string, string> ExtendedLogginData { get; set; }
        public long ContentLength { get; set; }
        public bool AllowZeroContentLength { get; set; }

        public RequestSettings()
        {
            RetryCount = 0;
            RetryDelayInMS = 0;
            Non200SuccessCodes = null;
        }
    }
}
