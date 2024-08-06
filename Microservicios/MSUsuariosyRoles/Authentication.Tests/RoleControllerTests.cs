using Authentication.Api.Controllers;
using Authentication.Application.Commands.Role.Create;
using Authentication.Application.Commands.Role.Delete;
using Authentication.Application.Commands.Role.Update;
using Authentication.Application.DTOs;
using Authentication.Application.Queries.Role;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Authentication.Tests
{
    public class RoleControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly RoleController _roleController;

        public RoleControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _roleController = new RoleController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsOkResult_WithInt()
        {
            // Arrange
            var command = new RoleCreateCommand
            {
                RoleName = "roletest"
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<RoleCreateCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _roleController.CreateRoleAsync(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task GetRoleAsync_ReturnsOkResult_WithListOfRoleResponseDTO()
        {
            // Arrange
            var expectedResponse = new List<RoleResponseDTO>
            {
                new RoleResponseDTO { Id = "1", RoleName = "Role1" },
                new RoleResponseDTO { Id = "2", RoleName = "Role2" },
                new RoleResponseDTO { Id = "3", RoleName = "Role3" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoleQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _roleController.GetRoleAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<RoleResponseDTO>>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ReturnsOkResult_WithRoleResponseDTO()
        {
            // Arrange
            var id = "1";
            var expectedResponse = new RoleResponseDTO
            {
                Id = "1",
                RoleName = "Role1"
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _roleController.GetRoleByIdAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RoleResponseDTO>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task DeleteRoleAsync_ReturnsOkResult_WithInt()
        {
            // Arrange
            var id = "1";
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _roleController.DeleteRoleAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task EditRole_ReturnsOkResult_WhenIdMatches()
        {
            // Arrange
            var id = "1";
            var command = new UpdateRoleCommand
            {
                Id = id,
                RoleName = "new role name"
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _roleController.EditRole(id, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task EditRole_ReturnsBadRequest_WhenIdDoesNotMatch()
        {
            // Arrange
            var id = "2";
            var command = new UpdateRoleCommand
            {
                Id = "1",
                RoleName = "testrole"
            };

            // Act
            var result = await _roleController.EditRole(id, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}