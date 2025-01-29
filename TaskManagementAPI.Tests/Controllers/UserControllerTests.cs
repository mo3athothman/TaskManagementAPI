using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.Models;
using Xunit;

namespace TaskManagementAPI.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            _userController = new UserController(_userManagerMock.Object);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction_WhenUserIsValid()
        {
            // Arrange
            var newUser = new User { Id = "1", UserName = "testUser", Email = "test@example.com" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userController.CreateUser(newUser) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("GetUser", result.ActionName);
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var testUser = new User { Id = "1", UserName = "testUser" };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(testUser);

            // Act
            var result = await _userController.GetUser("1") as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(testUser, result.Value);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            // Act
            var result = await _userController.GetUser("999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}