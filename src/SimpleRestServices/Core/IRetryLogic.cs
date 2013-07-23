using System;
using System.Collections.Generic;

namespace JSIStudios.SimpleRESTServices.Core
{
    public interface IRetryLogic<T, in T2>
    {
        T Execute(Func<T> logic, int retryCount = 1, TimeSpan? retryDelay = null);
        T Execute(Func<T> logic, IEnumerable<T2> sucessValues, int retryCount = 1, TimeSpan? retryDelay = null);
    }
}
