# UnisonMQ

**UnisonMQ: High-Performance Message Broker for Modern Applications** 🚀  
UnisonMQ is a lightweight yet powerful message broker designed for the .NET ecosystem. It combines ease of use, high performance, and flexibility, supporting both basic messaging scenarios and complex microservice architectures. Thanks to its modular design and open protocol, UnisonMQ integrates seamlessly into any infrastructure. 🌐  

---

### **Core Protocol Operations** 📡  
1. **`PING` → `PONG`** 💡  
   Connection health check. Clients send `PING`, and the server responds with `PONG` to confirm availability.  

2. **`SUB <subject> [Id]`** 🔍  
   Subscribe to a topic with wildcard support (`*` for single tokens, `>` for nested hierarchies). An optional `Id` allows managing multiple subscriptions within a single connection.  
   Example:  
   ```plaintext
   SUB orders.*.created 123
   ```

3. **`UNSUB <Id> [MaxMessages]`** 🚫  
   Unsubscribe using a subscription `Id`. If `MaxMessages` is specified, the broker will send up to that number of remaining messages before terminating the subscription.  

4. **`PUB <subject> <length>\r\n<message>`** 📤  
   Publish a message to a topic. The client specifies the data length (`length`) before sending the message body.  
   Example:  
   ```plaintext
   PUB alerts.system 11
   Hello World
   ```

5. **`EXIT`** 🔌  
   Gracefully terminate the connection. The server releases all client-associated resources.  

---

### **Docker Deployment** 🐳  
[UnisonMQ](https://hub.docker.com/r/magarich/unisonmq) is available as a preconfigured Docker image with metrics and port settings:  
```bash
docker run -e MetricsIp="+" -p 5888:5888 -p 5889:5889 --name unisonmq magarich/unisonmq
```  
- **Port 5888** ➡️ Primary port for message operations.  
- **Port 5889** 📊 Prometheus metrics endpoint (`http://localhost:5889/metrics`).  
- `MetricsIp="+"` 🌍 Enables metric collection on all network interfaces.  

---

### **C# Client Libraries** 📦  
Integrate UnisonMQ into your projects using NuGet packages:  
- [**Core Client**](https://www.nuget.org/packages/UnisonMQ.Client):  
  ```bash
  dotnet add package UnisonMQ.Client
  ```  
- [**Dependency Injection Support**](https://www.nuget.org/packages/UnisonMQ.DependencyInjection):  
  ```bash
  dotnet add package UnisonMQ.DependencyInjection
  ```  

**Example (Subscription and Publishing):** 💻  
```csharp
using UnisonMQ.Client;

await using var client = new UnisonMqClientService(new UnisonMqConfiguration());

if (!client.ConnectAsync())
{
    Console.WriteLine("Failed to connect.");
    return;
}

client.Subscribe<SensorData>("sensors.*", msg =>
{
    switch (msg.Data)
    {
        case TemplateSensorData data:
            Console.WriteLine($"Received temperature from {msg.Subject}: {data.Value}");
            break;
        default:
            Console.WriteLine($"Received message from {msg.Subject}: {msg.Data}");
            break;
    }
});
client.Publish("sensors.temperature", new TemplateSensorData(Guid.NewGuid(), "24.5°C"));

abstract class SensorData(Guid id)
{
    public Guid Id { get; } = id;
}

class TemplateSensorData(Guid id, string value) : SensorData(id)
{
    public string Value { get; } = value;
}
```

---

### **Monitoring Stack (Prometheus + Grafana)** 📈  
From the repository root:  
```bash
docker compose up -d
```  
- **Prometheus** 🎯 Collects metrics from UnisonMQ (port 5889).  
- **Grafana** 📉 Prebuilt dashboards for analyzing load, latency, and system health.  
  *Example Dashboards:* 👇  
  #### .NET Runtime metrics
![image](https://github.com/user-attachments/assets/9c534c65-2f76-4533-917e-f5d76294d8ec)
![image](https://github.com/user-attachments/assets/c4c6ff2b-64c3-4c27-8415-5fba773e6f12)

#### UnisonMQ metrics
![image](https://github.com/user-attachments/assets/f00e0f32-515b-4777-9055-25471bcc5754)
![image](https://github.com/user-attachments/assets/836a583a-9dac-4198-9eaf-2731c41a9736)

---

### **Telnet Compatibility** ⌨️  
UnisonMQ works with any TCP client, including Telnet:  
```bash
telnet localhost 5888
```  
**Example Session:** 💬  
```plaintext
> ping
< PONG
> SUB alerts.* 1
< +OK  
> PUB alerts.system 5  
> READY  
< MSG alerts.system 1 5
< READY
< +OK
> exit
< +OK
```

---

### **Prometheus Metrics** 📊  
UnisonMQ exposes detailed Prometheus metrics:  
- **Network Stats** 🌐 Active connections, traffic volume.  
- **Operations** ⚙️ PUB/SUB counts, processing times.  
- **Resource Usage** 💻 CPU, memory, GC metrics.  

```promql
# Example Query: Message publish rate 📤
rate(broker_messages_published_total[1m])
```

---

### **Why Choose UnisonMQ?** 🚀  
- Ultra-low latency ⚡ with an optimized protocol.  
- Wildcard support 🌟 for flexible topic management.  
- Subscription control 🔑 via `Id`.  
- Built-in monitoring 👁️ via Prometheus/Grafana.  
- Seamless .NET integration 🔄.  

Ideal for **IoT** 📶, **microservices** 🧩, and **real-time systems** ⏱️ where speed and reliability are critical!

---
### **License** 📜  
UnisonMQ is released under the **Apache License 2.0**.  
For full details, see the [LICENSE](LICENSE) file in the repository.  

### **Author** 👨💻  
- **[Magarich228](https://github.com/magarich228)** - Creator and maintainer of UnisonMQ.  

---

**Happy messaging!** 🎉  
