using System;
using Newtonsoft.Json;

namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonStringSerializer : IStringSerializer
    {
        public T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content,
                                                        new JsonSerializerSettings
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });
            }
            catch (JsonReaderException ex)
            {
                throw new StringSerializationException(ex);
            }
            catch (JsonSerializationException ex)
            {
                throw new StringSerializationException(ex);
            }
        }

        public string Serialize<T>(T obj)
        {
            
            try
            {
                return JsonConvert.SerializeObject(obj,
                                                   new JsonSerializerSettings
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
            }
            catch (JsonReaderException ex)
            {
                throw new StringSerializationException(ex);
            }
            catch (JsonSerializationException ex)
            {
                throw new StringSerializationException(ex);
            }
        }
    }

    public class StringSerializationException : Exception
    {
        public StringSerializationException(Exception innerException) : base(innerException.Message, innerException) {}
    }
}
