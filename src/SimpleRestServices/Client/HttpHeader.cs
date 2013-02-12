using System;

namespace JSIStudios.SimpleRESTServices.Client
{
    [Serializable]
    public class HttpHeader
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}