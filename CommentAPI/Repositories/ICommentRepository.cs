using CommentAPI.Models;

namespace CommentAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetCommentByIdAsync(long id);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(long postId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> PutCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(long id);
        // Helper method
        Task<bool> CommentExistsAsync(long id);
    }
}
