using System.ComponentModel.DataAnnotations;

namespace CommentAPI.Models.DTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }

        public string? Content { get; set; }

        public string AuthorId { get; set; }

        public Guid PostId { get; set; }
    }
}
