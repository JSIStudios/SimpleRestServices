using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIStudios.SimpleRESTServices.Client
{
    public interface IStringSerializer
    {
        T Deserialize<T>(string content);

        string Serialize<T>(T obj);
    }
}
