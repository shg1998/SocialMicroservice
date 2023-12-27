using CQRS.Core;
using CQRS.Core.Infrastructure;

namespace Post.Cmd.Infrastructure.Dispatchers;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly Dictionary<Type, Func<BaseCommand, Task>> _handlers = new();
    public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand
    {
        if (this._handlers.ContainsKey(typeof(T)))
            throw new IndexOutOfRangeException("You can not register the same command handler twice!");
        this._handlers.Add(typeof(T), x => handler((T)x));
    }

    public async Task SendAsync(BaseCommand command)
    {
        if (this._handlers.TryGetValue(command.GetType(), out Func<BaseCommand, Task> handler))
            await handler(command);
        else
            throw new ArgumentException(nameof(handler), "No Command handler was registered!");

    } 
}