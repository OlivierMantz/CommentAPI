using CommentAPI.Models;

namespace CommentAPI.Services
{
    public interface ICommentService
    {
        Task<Comment> GetCommentByIdAsync(Guid id);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> PutCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid id);
        // Helper method
        Task<bool> CommentExistsAsync(Guid id);
    }
}
