using System;
using System.Collections.Generic;
using System.Net;

namespace JSIStudios.SimpleRESTServices.Client
{
    public class RequestSettings
    {
        public virtual string ContentType { get; set; }
        public int RetryCount { get; set; }
        public TimeSpan RetryDelay { get; set; }
        public IEnumerable<HttpStatusCode> Non200SuccessCodes { get; set; }
        public virtual string Accept { get; set; }
        public Dictionary<HttpStatusCode, Action<Response>> ResponseActions { get; set; }
        public string UserAgent { get; set; }
        public ICredentials Credecials { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool ChunkRequest { get; set; }
        public Dictionary<string, string> ExtendedLogginData { get; set; }
        public long ContentLength { get; set; }
        public bool AllowZeroContentLength { get; set; }

        public RequestSettings()
        {
            RetryCount = 0;
            RetryDelay = TimeSpan.Zero;
            Non200SuccessCodes = null;
        }
    }
}
