using System.Text.Json;
using Quartz.Spi;

namespace AndNet.Manager.Server.Utility;

public class TextJsonSerializer : IObjectSerializer
{
    public void Initialize()
    {
    }

    public byte[] Serialize<T>(T obj) where T : class
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj);
    }

    public T? DeSerialize<T>(byte[] data) where T : class
    {
        return JsonSerializer.Deserialize<T?>(data);
    }
}