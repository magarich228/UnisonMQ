using System.Text;
using Newtonsoft.Json;

namespace UnisonMQ.Client;

internal static class Serialization
{
    public static byte[] Serialize(object data)
    {
        var json = JsonConvert.SerializeObject(data);

        return Encoding.UTF8.GetBytes(json);
    }
    
    public static T Deserialize<T>(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        
        return JsonConvert.DeserializeObject<T>(json) ?? 
               throw new UnisonMqClientException("Deserialization failed.");
    }
}