using System.Text;
using Newtonsoft.Json;

namespace UnisonMQ.Client;

internal static class Serialization
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };
    
    public static byte[] Serialize(object data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented, _settings);

        return Encoding.UTF8.GetBytes(json);
    }
    
    public static T Deserialize<T>(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        
        return JsonConvert.DeserializeObject<T>(json, _settings) ?? 
               throw new UnisonMqClientException("Deserialization failed.");
    }
}