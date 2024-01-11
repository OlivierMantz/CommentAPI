using CommentAPI.Models;

namespace CommentAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetCommentByIdAsync(Guid commentId);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid commentId);
        // Helper method
        Task<bool> CommentExistsAsync(Guid commentId);
    }
}