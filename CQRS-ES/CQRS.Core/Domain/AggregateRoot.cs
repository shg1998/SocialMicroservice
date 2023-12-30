using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public abstract class AggregateRoot
{
    private readonly List<BaseEvent> _changes = new();
    protected Guid _id;
    public Guid Id => this._id;

    public int Version { get; set; } = -1;

    public IEnumerable<BaseEvent> GetUncommittedChanges() =>
        this._changes;

    public void MarkChangesAsCommitted() =>
     this._changes.Clear();

    private void ApplyChanges(BaseEvent @event, bool isNew)
    {
        var method = this.GetType().GetMethod("Apply", [@event.GetType()]);
        if (method == null)
            throw new ArgumentNullException(nameof(method), $"The apply method wa not found in aggregate for {@event.GetType().Name}");

        method.Invoke(this, [@event]);
        if (isNew)
            this._changes.Add(@event);
    }

    protected void RaiseEvent(BaseEvent @event) => this.ApplyChanges(@event, true);

    public void ReplyEvents(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
            this.ApplyChanges(@event, false);
    }
}