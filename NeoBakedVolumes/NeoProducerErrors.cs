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
namespace NeoBakedVolumes;

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

public abstract class NeoErrors
{
    public NeoErrors()
    {
        MessageType = "NONE";
        ErrorTime = DateTimeOffset.Now;
        Machine = Environment.MachineName;
        User = Environment.UserName;
    }

    public string MessageType { get; protected set; }
    public DateTimeOffset ErrorTime { get; protected set; }
    public string Machine {get; protected set;}
    public string User { get; set; }

    public const string ErrorTopic = "neoerrors";

    public void SendMessage(bool noFlush = false)
    {
        try
        {
            var bootstrap = Environment.GetEnvironmentVariable("BROKER_BOOTSTRAP");
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrap,
                ClientId = $"{Environment.MachineName}_NeoVirtFS",
            };

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                // We'll have to add schema control (and supposedly have to use newtonsoftjson since it can't handle these for schemas)

                var jsonString = JsonSerializer.Serialize(this, options: new JsonSerializerOptions
                {
                    WriteIndented = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                });

                producer.Produce(ErrorTopic, new Message<string, string> { Key = MessageType, Value = jsonString });
                Console.WriteLine($"Send message: {jsonString}");

                if (noFlush)
                    producer.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessage Error: {ex.Message}");
        }
    }
}

