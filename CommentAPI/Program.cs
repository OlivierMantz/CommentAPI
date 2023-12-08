using BackEnd.Repositories;
using CommentAPI.Data;
using CommentAPI.Models;
using CommentAPI.Repositories;
using CommentAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

var secKey = builder.Configuration.GetValue<string>("Security:SecurityKey");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection")));


builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = "https://sublimewebapp.me/roles"

    };
});


var app = builder.Build();

CreateDB(app);

if (builder.Configuration.GetValue<bool>("RUN_MIGRATIONS_ON_STARTUP"))
{
    ApplyMigrations(app);
}


SeedDatabase(app);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

var cache = app.Services.GetService<IMemoryCache>();
cache.Set("SecurityKey", secKey);




void CreateDB(WebApplication app)
{

}
void ApplyMigrations(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        dbContext.Database.Migrate();
    }
}
void SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check if the database is empty
        if (!context.Comments.Any())
        {
            // Seed data
            context.Comments.AddRange(
                new Comment
                {
                    Content = "Nice image",
                    UserId = "1",
                    PostId = 1
                },
                new Comment
                {
                    Content = "Cool",
                    UserId = "2",
                    PostId = 1
                },
                new Comment
                {
                    Content = "Beautiful",
                    UserId = "2",
                    PostId = 2,
                }
            );
            context.SaveChanges();
        }
    }
}

