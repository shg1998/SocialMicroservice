using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregate;

namespace Post.Cmd.Infrastructure.Stores;

public class EventStore : IEventStore
{
    private readonly IEventStoreRepository _eventStoreRepository;
    private readonly IEventProducer _eventProducer;

    public EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer)
    {
        this._eventProducer = eventProducer;
        this._eventStoreRepository = eventStoreRepository;
    }


    public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
    {
        var eventStream = await this._eventStoreRepository.FindByAggregateId(aggregateId);
        if (eventStream == null || !eventStream.Any())
            throw new AggregateNotFoundException("Incorrect post id provided!");
        return eventStream.OrderBy(s => s.Version).Select(s => s.EventData).ToList();
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
    {
        var eventStream = await this._eventStoreRepository.FindByAggregateId(aggregateId);
        if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
            throw new ConcurrencyException();
        var version = expectedVersion;
        foreach (var @event in events)
        {
            version++;
            @event.Version = version;
            var eventType = @event.GetType().Name;
            var eventModel = new EventModel
            {
                Timestamp = DateTime.Now,
                AggregateIdentifier = aggregateId,
                AggregateType = nameof(PostAggregate),
                Version = version,
                EventType = eventType,
                EventData = @event
            };
            await this._eventStoreRepository.SaveAsync(eventModel);
            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            await this._eventProducer.ProduceAsync(topic!, @event);
        }
    }
}