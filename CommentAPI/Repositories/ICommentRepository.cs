using CommentAPI.Models;

namespace CommentAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetCommentByIdAsync(Guid id);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid id);
        // Helper method
        Task<bool> CommentExistsAsync(Guid id);
    }
}