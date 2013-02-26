using System;
using System.Collections.Specialized;
using JSIStudios.SimpleRESTServices.Server.EventArgs;

namespace JSIStudios.SimpleRESTServices.Core
{
    public interface IRequestProcessor
    {
        event EventHandler<RESTRequestStartedEventArgs> RequestStarted;
        event EventHandler<RESTRequestCompletedEventArgs> RequestCompleted;
        event EventHandler<RESTRequestErrorEventArgs> OnError;

        void Execute(Action<Guid> callBack);
        void Execute(Action<Guid> callBack, NameValueCollection responseHeaders);

        TResult Execute<TResult>(Func<Guid, TResult> callBack);
        TResult Execute<TResult>(Func<Guid, TResult> callBack, NameValueCollection responseHeaders);
    }
}