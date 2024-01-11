using CommentAPI.Models;
using CommentAPI.Models.DTOs;

namespace CommentAPI.Services
{
    public interface ICommentService
    {
        Task<Comment> GetCommentByIdAsync(Guid commentId);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId);
        Task<Comment> CreateCommentAsync(Guid postId, CreateCommentDTO createCommentDTO, string authorId);
        Task<bool> UpdateCommentAsync(Guid commentId, CreateCommentDTO createCommentDTO, string authorId);
        Task<bool> DeleteCommentAsync(Guid commentId, string userId);
        // Helper method
    }
}
