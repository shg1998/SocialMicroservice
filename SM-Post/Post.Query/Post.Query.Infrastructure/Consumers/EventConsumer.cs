using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;

namespace Post.Query.Infrastructure.Consumers;

public class EventConsumer : IEventConsumer
{
    private readonly ConsumerConfig _config;
    private readonly IEventHandler _eventHanlder;

    public EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler)
    {
        this._config = config.Value;
        this._eventHanlder = eventHandler;
    }

    public void Consume(string topic)
    {
        using var consumer = new ConsumerBuilder<string, string>(this._config)
        .SetKeyDeserializer(Deserializers.Utf8)
        .SetValueDeserializer(Deserializers.Utf8)
        .Build();

        consumer.Subscribe(topic);

        while (true)
        {
            var consumeResult = consumer.Consume();
            if (consumeResult?.Message == null) continue;

            var options = new JsonSerializerOptions
            {
                Converters = { new EventJsonConverter() }
            };

            var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);
            var handlerMethod = this._eventHanlder.GetType().GetMethod("On", [@event.GetType()]);
            if (handlerMethod == null)
                throw new ArgumentException(nameof(handlerMethod), "Could not find event handler method!");
            handlerMethod.Invoke(this._eventHanlder, [@event]);
            consumer.Commit(consumeResult);
        }
    }
}