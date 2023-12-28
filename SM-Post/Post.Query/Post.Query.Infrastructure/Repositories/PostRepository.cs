using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly DatabaseContextFactory _contextFactory;
    public PostRepository(DatabaseContextFactory contextFactory) => this._contextFactory = contextFactory;
    public async Task CreateAsync(PostEntity post)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        await context.Posts.AddAsync(post);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid postId)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        var post = await GetByIdAsync(postId);
        if (post == null) return;
        context.Posts.Remove(post);
        await context.SaveChangesAsync();
    }

    public async Task<PostEntity> GetByIdAsync(Guid postId)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        return await context.Posts.Include(s => s.Comment)
        .FirstOrDefaultAsync(x => x.PostId == postId);
    }

    public async Task<List<PostEntity>> ListAllAsync()
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
        .Include(c => c.Comment)
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<List<PostEntity>> ListByAuthorAsync(string author)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
        .Include(c => c.Comment)
        .AsNoTracking()
        .Where(s => s.Author.Contains(author))
        .ToListAsync();
    }

    public async Task<List<PostEntity>> ListWithCommentsAsync()
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
        .Include(c => c.Comment)
        .AsNoTracking()
        .Where(s => s.Comment != null && s.Comment.Any())
        .ToListAsync();
    }

    public async Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
        .Include(c => c.Comment)
        .AsNoTracking()
        .Where(s => s.Likes >= numberOfLikes)
        .ToListAsync();
    }

    public async Task UpdateAsync(PostEntity post)
    {
        using DatabaseContext context = this._contextFactory.CreateDbContext();
        context.Posts.Update(post);
        _ = await context.SaveChangesAsync();
    }
}