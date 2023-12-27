using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregate;

namespace Post.Cmd.Infrastructure.Handlers;

public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
{
    private readonly IEventStore _eventStore;

    public EventSourcingHandler(IEventStore eventStore) => this._eventStore = eventStore;

    public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
    {
        var aggregate = new PostAggregate();
        var events = await this._eventStore.GetEventsAsync(aggregateId);
        if(events == null || !events.Any()) return aggregate;
        aggregate.ReplyEvents(events);
        var latestVersion = events.Select(s=> s.Version).Max();
        aggregate.Version = latestVersion;
        return aggregate;
    }

    public async Task SaveAsync(AggregateRoot aggregate)
    {
        await this._eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version);
        aggregate.MarkChangesAsCommitted();
    }
}