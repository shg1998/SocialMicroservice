using Post.Common.Events;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Infrastructure.Handlers;

public class EventHandler : IEventHandler
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentReoisitory;

    public EventHandler(IPostRepository postRepository, ICommentRepository commentRepository)
    {
        this._postRepository = postRepository;
        this._commentReoisitory = commentRepository;
    }

    public async Task On(PostCreatedEvent @event)
    {
        var post = new PostEntity
        {
            PostId = @event.Id,
            Author = @event.Author,
            DatePosted = @event.DatePosted,
            Message = @event.Message
        };

        await this._postRepository.CreateAsync(post);
    }

    public async Task On(MessageUpdatedEvent @event)
    {
        var post = await this._postRepository.GetByIdAsync(@event.Id);
        if (post == null) return;
        post.Message = @event.Message;
        await this._postRepository.UpdateAsync(post);
    }

    public async Task On(PostLikedEvent @event)
    {
        var post = await this._postRepository.GetByIdAsync(@event.Id);
        if (post == null) return;
        post.Likes++;
        await this._postRepository.UpdateAsync(post);
    }

    public async Task On(CommentAddedEvent @event)
    {
        var comment = new CommentEntity {
            PostId = @event.Id,
            CommentId = @event.CommentId,
            CommentDate = @event.CommentDate,
            Comment = @event.Comment,
            Username = @event.Username,
            Edited = false
        };
        await this._commentReoisitory.CreateAsync(comment);
    }

    public async Task On(CommentUpdatedEvent @event)
    {
        var comment = await this._commentReoisitory.GetByIdAsync(@event.CommentId);
        if(comment == null) return;
        comment.Comment = @event.Comment;
        comment.Edited = true;
        comment.CommentDate = @event.EditDate;
        await this._commentReoisitory.UpdateAsync(comment);
    }

    public async Task On(CommentRemovedEvent @event) =>
     await this._commentReoisitory.DeleteAsync(@event.CommentId);

    public async Task On(PostRemovedEvent @event) =>
     await this._postRepository.DeleteAsync(@event.Id);
}