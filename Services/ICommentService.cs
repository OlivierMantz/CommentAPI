using CommentAPI.Models;

namespace CommentAPI.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsAsync();
        Task<Comment> GetCommentByIdAsync(long id);
        Task PostCommentAsync(Comment comment);
        Task<bool> PutCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(long id);
        // Helper method
        Task<bool> CommentExistsAsync(long id);
    }
}
