using System;
using System.Diagnostics;

namespace JSIStudios.SimpleRESTServices.Client
{
    [Serializable]
    [DebuggerDisplay("{Key,nq} = {Value,nq}")]
    public class HttpHeader
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}