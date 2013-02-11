using System;
using System.Collections.Specialized;

namespace JSIStudios.SimpleRESTServices.Server
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

        void ExecuteSecure(Action<Guid> callBack);
        void ExecuteSecure(Action<Guid> callBack, string controllerName);
        void ExecuteSecure(Action<Guid> callBack, string controllerName, NameValueCollection responseHeaders);

        TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack);
        TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack, string controllerName);
        TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack, string controllerName, NameValueCollection responseHeaders);

    }
}