using System.ComponentModel.DataAnnotations;

namespace Verum.CrossCutting.Mensageria;

public sealed class RabbitMqOptions
{
  public const string SectionName = "RabbitMQ";
  public bool Enabled { get; set; }
  [Required] public string Host { get; set; } = "localhost";
  [Required] public string VirtualHost { get; set; } = "/";
  [Required] public string Username { get; set; } = "";
  [Required] public string Password { get; set; } = "";
  public bool UseSsl { get; set; }
  [Range(1, 65535)] public int Port { get; set; } = 5672;
  [Range(1, 65535)] public int PrefetchCount { get; set; } = 16;
  [Range(1, 1024)] public int ConcurrentMessageLimit { get; set; } = 8;
  [Range(0, 10)] public int RetryCount { get; set; } = 3;
  [Range(1, 120)] public int PublishTimeoutSeconds { get; set; } = 15;
  public bool UseDelayedRedelivery { get; set; }
  public bool UseInMemoryOutbox { get; set; } = true;
  public int[] RedeliveryIntervalsSeconds { get; set; } = [30, 120, 600];
}
