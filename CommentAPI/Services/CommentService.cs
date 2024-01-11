using CommentAPI.Models.DTOs;
using CommentAPI.Models;
using CommentAPI.Repositories;
using CommentAPI.Services;
using System.ComponentModel.Design;

namespace CommentAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _commentRepository.GetAllCommentsAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId)
        {
            return await _commentRepository.GetAllCommentsInPostAsync(postId);
        }
        public async Task<Comment> GetCommentByIdAsync(Guid commentId)
        {
            return await _commentRepository.GetCommentByIdAsync(commentId);
        }

        public async Task<Comment> CreateCommentAsync(Guid postId, CreateCommentDTO createCommentDTO, string authorId)
        {
            if (string.IsNullOrEmpty(createCommentDTO?.Content) || createCommentDTO.Content.Length < 3)
            {
                throw new ArgumentException("Comment content cannot be empty or smaller than 3 characters long.");
            }

            var comment = new Comment
            {
                Content = createCommentDTO.Content,
                AuthorId = authorId,
                PostId = postId
            };

            return await _commentRepository.CreateCommentAsync(comment);
        }

        public async Task<bool> UpdateCommentAsync(Guid commentId, CreateCommentDTO createCommentDTO, string authorId)
        {
            var existingComment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (existingComment == null)
            {
                return false;
            }

            if (existingComment.AuthorId != authorId)
            {
                throw new UnauthorizedAccessException("User is not authorized to update this comment.");
            }

            if (string.IsNullOrEmpty(createCommentDTO?.Content))
            {
                throw new ArgumentException("Comment content cannot be empty.");
            }

            existingComment.Content = createCommentDTO.Content;
            return await _commentRepository.UpdateCommentAsync(existingComment);
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, string authorId)
        {
            if (string.IsNullOrEmpty(authorId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return false;
            }

            if (comment.AuthorId != authorId)
            {
                throw new UnauthorizedAccessException("User is not authorized to delete this comment.");
            }

            return await _commentRepository.DeleteCommentAsync(commentId);
        }
    }
}