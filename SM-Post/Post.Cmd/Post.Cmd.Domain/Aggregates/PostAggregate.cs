
using CQRS.Core.Domain;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregate;

public class PostAggregate : AggregateRoot
{

    #region fields
    private string _author;
    private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();
    #endregion

    #region properties
    private bool _active;
    public bool Active
    {
        get => this._active;
        set => this._active = value;
    }
    #endregion

    public PostAggregate(Guid id, string author, string message)
    {
        this.RaiseEvent(new PostCreatedEvent
        {
            Id = id,
            Author = author,
            Message = message,
            DatePosted = DateTime.Now
        });
    }

    #region public methods

    public void Apply(PostCreatedEvent @event)
    {
        this._id = @event.Id;
        this._active = true;
        this._author = @event.Author;
    }

    public void EditMessage(string message)
    {
        this.CheckForActiveOrInActive();
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException($"The Value of {nameof(message)} can not be null or empty. Please provide a valid {nameof(message)}!");
        this.RaiseEvent(new MessageUpdatedEvent
        {
            Id = this._id,
            Message = message
        });
    }

    public void Apply(MessageUpdatedEvent @event) => this._id = @event.Id;

    public void LikePost()
    {
        this.CheckForActiveOrInActive();
        this.RaiseEvent(new PostLikedEvent
        {
            Id = this._id
        });
    }

    public void Apply(PostLikedEvent @event) => this._id = @event.Id;

    public void AddComment(string comment, string username)
    {
        this.CheckForActiveOrInActive();

        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException($"The Value of {nameof(comment)} can not be null or empty. Please provide a valid {nameof(comment)}!");
        this.RaiseEvent(new CommentAddedEvent
        {
            Id = this._id,
            CommentId = Guid.NewGuid(),
            Comment = comment,
            Username = username,
            CommentDate = DateTime.Now
        });
    }

    public void Apply(CommentAddedEvent @event)
    {
        this._id = @event.Id;
        this._comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
    }

    public void EditComment(Guid commentId, string comment, string username)
    {
        this.CheckForActiveOrInActive();
        if (!this._comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            throw new InvalidOperationException("You are not allowed to edit a comment that wa created by another user!");
        this.RaiseEvent(new CommentUpdatedEvent
        {
            Id = this._id,
            CommentId = commentId,
            Comment = comment,
            Username = username,
            EditDate = DateTime.Now
        });
    }

    public void Apply(CommentUpdatedEvent @event)
    {
        this._id = @event.Id;
        this._comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.Username);
    }

    public void RemoveComment(Guid commentId, string username)
    {
        this.CheckForActiveOrInActive();
        if (!this._comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            throw new InvalidOperationException("You are not allowed to remove a comment that wa created by another user!");
        this.RaiseEvent(new CommentRemovedEvent
        {
            CommentId = commentId,
            Id = this._id
        });
    }

    public void Apply(CommentRemovedEvent @event)
    {
        this._id = @event.Id;
        this._comments.Remove(@event.CommentId);
    }

    public void DeletePost(string username)
    {
        this.CheckForActiveOrInActive();
        if (!this._author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            throw new InvalidOperationException("You are not allowed to remove a post that created by another user!");
        this.RaiseEvent(new PostRemovedEvent
        {
            Id = this._id,
        });
    }

    public void Apply(PostRemovedEvent @event)
    {
        this._id = @event.Id;
        this._active = false;
    }

    #endregion

    #region private methods
    private void CheckForActiveOrInActive()
    {
        if (!this._active)
            throw new InvalidOperationException("You can not do any ops because you have inactive status!");

    }
    #endregion
}