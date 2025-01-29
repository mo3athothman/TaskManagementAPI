using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Xunit;

namespace TaskManagementAPI.Tests.Controllers
{
    public class TaskControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskController _taskController;
        private readonly Mock<UserManager<User>> _userManagerMock;

        public TaskControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var testUser = new User { Id = "1", UserName = "testUser", Email = "test@example.com" };

            // Ensure `UserManager.GetUserAsync()` always returns a valid user
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            _taskController = new TaskController(_context, _userManagerMock.Object);
        }

        [Fact]
        public async Task CreateTask_ReturnsCreatedAtAction_WhenTaskIsValid()
        {
            // Ensure no duplicate IDs by clearing the database
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();

            // Arrange
            var newTask = new TaskItem { Title = "Test Task", Description = "Sample", Status = "Pending" };

            // Act
            var result = await _taskController.CreateTask(newTask) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("GetTask", result.ActionName);
        }

        [Fact]
        public async Task GetTask_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var testUser = new User { Id = "1", UserName = "testUser" };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            var testTask = new TaskItem { Id = 1, Title = "Test Task", Status = "Pending", AssignedUserId = "1" };
            _context.Tasks.Add(testTask);
            await _context.SaveChangesAsync();

            // Ensure `UserManager.GetUserAsync()` returns the test user
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            // Mock User Identity to simulate an authenticated user
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, testUser.Id) };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
            _taskController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _taskController.GetTask(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _taskController.GetTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
        {
            // Arrange
            var testTask = new TaskItem { Id = 1, Title = "Task to Delete", Status = "Pending" };
            _context.Tasks.Add(testTask);
            _context.SaveChanges();

            // Act
            var result = await _taskController.DeleteTask(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}