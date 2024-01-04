using Xunit;
using CommentAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommentAPI.Models;
using Microsoft.EntityFrameworkCore;
using BackEnd.Repositories;
using CommentAPI.Data;
using CommentAPI.Models.DTOs;
using CommentAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using Moq;

namespace CommentAPI.Controllers.Tests
{
    public class CommentsControllerIntegrationTests
    {
        private Guid CommentId1;
        private Guid CommentId2;
        private Guid CommentId3;
        private Guid PostId1;
        private Guid PostId2;
        private Guid PostId3;

        public CommentsControllerIntegrationTests()
        {
            SetupTestData();
        }
        private void SetupTestData()
        {
            CommentId1 = Guid.NewGuid();
            CommentId2 = Guid.NewGuid();
            CommentId3 = Guid.NewGuid();
            PostId1 = Guid.NewGuid();
            PostId2 = Guid.NewGuid();
            PostId3 = Guid.NewGuid();

            using var context = new ApplicationDbContext(CreateNewContextOptions());
            PopulateTestData(context);
        }
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            // Create a unique name for each in-memory database
            string dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return options;
        }

        private void PopulateTestData(ApplicationDbContext context)
        {
            var testData = new[]
            {
                new Comment { Id = CommentId1, Content = "Nice image", AuthorId = "1", PostId = PostId1 },
                new Comment { Id = CommentId2, Content = "Cool", AuthorId = "2", PostId = PostId2 },
                new Comment { Id = CommentId3, Content = "Cool", AuthorId = "3", PostId = PostId3 },
            };

            context.Comments.AddRange(testData);
            context.SaveChanges();
        }

        private void MockUserAuthentication(CommentsController controller, string authorId="1", string role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authorId)
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };
        }

        [Fact]
        public async Task GetAllComments_Admin()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, "AdminUserId", "Admin");

            var result = await controller.GetAllCommentsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }

        //[Theory]
        //[InlineData("1")]
        //public async Task GetAllComments_NotAdmin_Forbidden(string userId)
        //{
        //    var options = CreateNewContextOptions();
        //    using var context = new ApplicationDbContext(options);
        //    PopulateTestData(context);

        //    var commentRepository = new CommentRepository(context);
        //    var commentService = new CommentService(commentRepository);
        //    var controller = new CommentsController(commentService);

        //    MockUserAuthentication(controller, userId);

        //    var result = await controller.GetAllCommentsAsync();

        //    Assert.IsType<ForbidResult>(result.Result);
        //}

        [Fact]
        public async Task GetAllComments_InPost()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var result = await controller.GetAllCommentsInPost(PostId1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }

        [Theory]
        [InlineData()]
        public async Task GetAllComments_InPost_InvalidPostId_ReturnsEmpty()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var result = await controller.GetAllCommentsInPost(new Guid());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.False(comments.Any());
        }

        [Theory]
        [InlineData()]
        public async Task Post_CreateComment()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = "Great comment!"
            };

            var result = await controller.CreateCommentAsync(PostId1, createCommentDTO);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var commentDto = Assert.IsType<CommentDTO>(okResult.Value);

            Assert.Equal(createCommentDTO.Content, commentDto.Content);
            Assert.Equal(PostId1, commentDto.PostId);

            var savedComment = await context.Comments.FirstOrDefaultAsync(c => c.Content == createCommentDTO.Content && c.PostId == PostId1);
            Assert.NotNull(savedComment);
        }

        [Theory]
        [InlineData()]
        public async Task Post_CreateComment_InvalidData_ReturnsBadRequest()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = ""
            };
            var result = await controller.CreateCommentAsync(PostId1, createCommentDTO);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData()]
        public async Task DeleteComment_Admin()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var result = await controller.DeleteComment(CommentId1);

            Assert.IsType<NoContentResult>(result);
            var deletedComment = await context.Comments.FindAsync(CommentId1);
            Assert.Null(deletedComment);
        }

        [Theory]
        [InlineData("2")]
        public async Task DeleteComment_OwnComment(string authorId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, authorId);

            // CommentId retrieved via debugging for now
            //var result1 = await controller.GetAllCommentsInPost(1);

            var result = await controller.DeleteComment(CommentId2);

            Assert.IsType<NoContentResult>(result);
            var deletedComment = await context.Comments.FindAsync(CommentId2);
            Assert.Null(deletedComment);
        }

        [Theory]
        [InlineData("2")]
        public async Task DeleteComment_OtherUser_Forbidden(string authorId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, authorId);

            var result = await controller.DeleteComment(CommentId1);

            Assert.IsType<ForbidResult>(result);
        }

    }
}