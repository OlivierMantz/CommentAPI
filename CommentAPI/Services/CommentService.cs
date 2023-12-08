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


        public async Task<IEnumerable<Comment>> GetCommentsAsync()
        {
            return await _commentRepository.GetCommentsAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(long postId)
        {
            return await _commentRepository.GetAllCommentsInPostAsync(postId);
        }

        public async Task<Comment> GetCommentByIdAsync(long id)
        {
            return await _commentRepository.GetCommentByIdAsync(id);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _commentRepository.CreateCommentAsync(comment);
            return comment;
        }

        public async Task<bool> PutCommentAsync(Comment comment)
        {
            return await _commentRepository.PutCommentAsync(comment);
        }

        public async Task<bool> DeleteCommentAsync(long id)
        {
            return await _commentRepository.DeleteCommentAsync(id);
        }

        public async Task<bool> CommentExistsAsync(long id)
        {
            return await _commentRepository.CommentExistsAsync(id);
        }
    }
}
