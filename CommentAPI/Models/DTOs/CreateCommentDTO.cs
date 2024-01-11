using System.ComponentModel.DataAnnotations;

namespace CommentAPI.Models.DTOs
{
    public class CreateCommentDTO
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Content { get; set; } 
    }
}
