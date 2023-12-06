using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommentAPI.Models
{
    public class Comment
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long PostId { get; set; }
    }
}
