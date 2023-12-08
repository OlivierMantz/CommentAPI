﻿using System.ComponentModel.DataAnnotations;

namespace CommentAPI.Models.DTOs
{
    public class CommentDTO
    {
        public long Id { get; set; }

        public string? Content { get; set; }

        public string UserId { get; set; }

        public long? PostId { get; set; }
    }
}
