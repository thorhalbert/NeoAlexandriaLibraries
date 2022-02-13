using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// These are the kafka/redpanda event messages that filesystem activities will generate
/// This might also include the marshall/send infrastructure
/// </summary>
namespace NeoVirtFS.Events;

// New File Closed
// Directory created
// File deleted
// File renamed
// Directory removed
// Create volume
// File Annealed
// File Needs to Bake
// File Baked
// Scry Changed (or possibily new)
// Error

public abstract class FileActivityBase
{
    public FileActivityBase()
    {
        MessageType = "NONE";
        AssetSha1 = Array.Empty<byte>();
    }

    public string MessageType { get; set; }

    public string VolumeId { get; set; }
    public string FileId { get; set; }
    public DateTimeOffset EventTime { get; set; }
    public string ServerName { get; set; }
    public Byte[] AssetSha1 { get; set; }

    public void SendMessage()
    {
        try
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "cirdan:9092",
                ClientId = Dns.GetHostName(),
            };

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                // We'll have to add schema control (and supposedly have to use newtonsoftjson since it can't handle these for schemas)

                string jsonString = JsonSerializer.Serialize(this, options: new JsonSerializerOptions
                {
                    WriteIndented = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                });

                producer.Produce("neofiles", new Message<string, string> { Key = MessageType, Value = jsonString });
                Console.WriteLine($"Send message {jsonString}");
                producer.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessage Error: {ex.Message}");
        }
    }
}

public class Event_FileAnnealed : FileActivityBase
{
    public Event_FileAnnealed()
    {
        MessageType = "Event_FileAnnealed";
    }
}
public class Event_FileNeedsBaked : FileActivityBase
{
    public Event_FileNeedsBaked()
    {
        MessageType = "Event_FileNeedsBaked";
    }
}