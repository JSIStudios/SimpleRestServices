using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace SimpleRestServices.Server
{
    public class RESTRequestProcessor : IRequestProcessor
    {
        public event EventHandler<RESTRequestStartedEventArgs> RequestStarted;
        public event EventHandler<RESTRequestCompletedEventArgs> RequestCompleted;

        public event EventHandler<RESTRequestErrorEventArgs> OnError;

        #region Interface methods
        
        public virtual void ExecuteSecure(Action<Guid> callBack) 
        {
            ExecuteSecure(callBack, string.Empty);
        }

        public virtual void ExecuteSecure(Action<Guid> callBack, string controllerName)
        {
            ExecuteSecure(callBack, controllerName, null);
        }

        public virtual void ExecuteSecure(Action<Guid> callBack, string controllerName, NameValueCollection responseHeaders)
        {
            try
            {
                if (!IsRequestAuthorized())
                {
                    ThrowWebFaultException(HttpStatusCode.Unauthorized);
                }
            }
            catch (HttpHeaderNotFoundException ex)
            {
                ThrowWebFaultException<string>(string.Format("Http Header Not Found: {0}", ex.Name), HttpStatusCode.BadRequest);
            }

            Execute(callBack, responseHeaders);
        }

        public virtual TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack)
        {
            return ExecuteSecure(callBack, string.Empty);
        }

        public virtual TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack, string controllerName)
        {
            return ExecuteSecure(callBack, controllerName, null);
        }

        public virtual TResult ExecuteSecure<TResult>(Func<Guid, TResult> callBack, string controllerName, NameValueCollection responseHeaders)
        {
            try
            {
                if (!IsRequestAuthorized())
                {
                    ThrowWebFaultException(HttpStatusCode.Unauthorized);
                }
            }
            catch (HttpHeaderNotFoundException ex)
            {
                ThrowWebFaultException<string>(string.Format("Http Header Not Found: {0}", ex.Name), HttpStatusCode.BadRequest);
            }

            return Execute(callBack, responseHeaders);
        }

        public virtual void Execute(Action<Guid> callBack)
        {
            Execute(callBack, null);
        }

        public virtual void Execute(Action<Guid> callBack, NameValueCollection responseHeaders)
        {
            ExecuteSafely<object>((requestId) =>
            {
                callBack(requestId);
                return null;
            }, responseHeaders);
        }

        public virtual TResult Execute<TResult>(Func<Guid, TResult> callBack)
        {
            return Execute(callBack, null);
        }

        public virtual TResult Execute<TResult>(Func<Guid, TResult> callBack, NameValueCollection responseHeaders)
        {
            TResult result = default(TResult);

            ExecuteSafely((requestId) =>
            {
                result = callBack(requestId);
                return result;
            }, responseHeaders);

            if (WebOperationContext.Current != null && WebOperationContext.Current.OutgoingResponse.StatusCode == HttpStatusCode.OK)
                if (result == null)
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
            
            return result;
        }

        #endregion

        #region Private methods

        private void ExecuteSafely<TResult>(Func<Guid, TResult> callBack, NameValueCollection responseHeaders)
        {
            var requestId = Guid.NewGuid();

            try
            {
                if (RequestStarted != null)
                    RequestStarted(this, new RESTRequestStartedEventArgs(requestId, GetHttpRequest(HttpContext.Current.Request)));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = callBack(requestId);

                stopwatch.Stop();

                if (RequestCompleted != null)
                    RequestCompleted(this, new RESTRequestCompletedEventArgs(requestId, result, stopwatch.ElapsedMilliseconds));

                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                    if (responseHeaders != null)
                        WebOperationContext.Current.OutgoingResponse.Headers.Add(responseHeaders);
                }
            }
            catch (BadWebRequestException ex)
            {
                if (OnError != null)
                    OnError(this, new RESTRequestErrorEventArgs(requestId, ex));

                ThrowWebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (HttpResourceNotFoundException ex)
            {
                if (OnError != null)
                    OnError(this, new RESTRequestErrorEventArgs(requestId, ex));

                ThrowWebFaultException<string>(ex.Message, HttpStatusCode.NotFound);
            }
            catch (HttpResourceNotModifiedException ex)
            {
                if (OnError != null)
                    OnError(this, new RESTRequestErrorEventArgs(requestId, ex));

                if (WebOperationContext.Current != null)
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotModified;
                else
                    ThrowWebFaultException(HttpStatusCode.NotModified);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, new RESTRequestErrorEventArgs(requestId, ex));

                if (WebOperationContext.Current != null)
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;

                ThrowWebFaultException<string>(string.Format("There was an error processing the request:{0}", ex.Message), HttpStatusCode.InternalServerError);
            }
        }

        private static string GetHttpRequest(HttpRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} {1}", request.RequestType, request.RawUrl));
            sb.AppendLine(request.ServerVariables["ALL_RAW"]);

            return sb.ToString();
        }

        private bool IsRequestAuthorized()
        {
            throw new NotImplementedException();
        }

        private void ThrowWebFaultException<T>(T value, HttpStatusCode statusCode)
        {
            throw new WebFaultException<T>(value, statusCode);
        }

        private void ThrowWebFaultException(HttpStatusCode statusCode)
        {
            throw new WebFaultException(statusCode);
        }

        #endregion
    }


    public class RESTRequestStartedEventArgs : EventArgs
    {
        public Guid RequestId { get; private set; }

        public string Request { get; private set; }

        public RESTRequestStartedEventArgs(Guid requestId, string request)
            : base()
        {
            RequestId = requestId;
            Request = request;
        }
    }

    public class RESTRequestErrorEventArgs : EventArgs
    {
        public Guid RequestId { get; private set; }

        public Exception Error { get; private set; }

        public RESTRequestErrorEventArgs(Guid requestId, Exception error)
            : base()
        {
            RequestId = requestId;
            Error = error;
        }
    }

    public class RESTRequestCompletedEventArgs : EventArgs
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

    public class BadWebRequestException : Exception
    {
        public BadWebRequestException(string message)
            : base(message)
        {
        }

        public BadWebRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class HttpHeaderNotFoundException : Exception
    {
        public string Name { get; set; }
        public HttpHeaderNotFoundException(string name, string message)
            : base(message)
        {
            Name = name;
        }

        public HttpHeaderNotFoundException(string name)
        {
            Name = name;
        }

        public HttpHeaderNotFoundException()
        {
        }
    }

    public class HttpResourceNotFoundException : Exception
    {
        public HttpResourceNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class HttpResourceNotModifiedException : Exception
    {
        public HttpResourceNotModifiedException()
        {
        }
    }
}
