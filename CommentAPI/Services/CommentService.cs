using CommentAPI.Repositories;
using CommentAPI.Models;

namespace CommentAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<Comment> GetCommentByIdAsync(Guid id)
        {
            return await _commentRepository.GetCommentByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _commentRepository.GetAllCommentsAsync();
        }  

        public async Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId)
        {
            return await _commentRepository.GetAllCommentsInPostAsync(postId);
        }

        public async Task<Comment> CreateCommentAsync(Comment Comment)
        {
            await _commentRepository.CreateCommentAsync(Comment);
            return Comment;
        }

        public async Task<bool> PutCommentAsync(Comment Comment)
        {
            return await _commentRepository.PutCommentAsync(Comment);
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            return await _commentRepository.DeleteCommentAsync(id);
        }

        public async Task<bool> CommentExistsAsync(Guid id)
        {
            return await _commentRepository.CommentExistsAsync(id);
        }
    }
}
