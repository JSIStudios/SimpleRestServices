using System;

namespace JSIStudios.SimpleRESTServices.Server.EventArgs
{
    public class RESTRequestCompletedEventArgs : System.EventArgs
    {
        public Guid RequestId { get; private set; }

        public object Response { get; private set; }

        public long ExecutionTime { get; private set; }

        public RESTRequestCompletedEventArgs(Guid requestId, object response, long exectionTime)
            : base()
        {
            RequestId = requestId;
            Response = response;
            ExecutionTime = exectionTime;

        }
    }
}