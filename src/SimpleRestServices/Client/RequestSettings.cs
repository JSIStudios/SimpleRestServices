using System;
using System.Collections.Generic;
using System.Net;

namespace JSIStudios.SimpleRESTServices.Client
{
    public abstract class RequestSettings
    {
        public abstract string ContentType { get; set; }
        public int RetryCount { get; set; }
        public int RetryDelayInMS { get; set; }
        public IEnumerable<int> Non200SuccessCodes { get; set; }
        public abstract string Accept { get; set; }
        public Dictionary<int, Action<Response>> ResponseActions { get; set; }
        public string UserAgent { get; set; }
        public ICredentials Credecials { get; set; }

        protected RequestSettings()
        {
            RetryCount = 0;
            RetryDelayInMS = 0;
            Non200SuccessCodes = null;
        }
    }
}
