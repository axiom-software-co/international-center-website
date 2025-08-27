using Aspire.Hosting;
using SharedPlatform.Features.Messaging.Configuration;

namespace AspireHost.Features.ResourceOrchestration;

public class MessagingResource
{
    public static void AddRabbitMQ(IDistributedApplicationBuilder builder, string name = "rabbitmq")
    {
        throw new NotImplementedException();
    }

    public static void ConfigureMessagingOptions(IDistributedApplicationBuilder builder, MessagingOptions options)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureRabbitMQSettings(IDistributedApplicationBuilder builder, RabbitMqConfiguration config)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureMassTransit(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddMessageQueues(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}