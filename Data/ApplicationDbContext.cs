using CommentAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommentAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments { get; set; }
}