using Core.Services.MSPermisos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MSPermisos.Api.Controllers;

namespace MSPermisos.Api.Tests
{
    public class PermisosControllerTests
    {
        //private readonly Mock<IPermisoService> _mockService;
        //private readonly PermisosController _controller;

        //public PermisosControllerTests()
        //{
        //    _mockService = new Mock<IPermisoService>();
        //    _controller = new PermisosController(_mockService.Object);
        //}

        //[Fact]
        //public async Task GetAll_ReturnsOkResult_WithListOfPermisos()
        //{
        //    // Arrange
        //    var permisos = new List<PermisoResponseDTO> { new PermisoResponseDTO(), new PermisoResponseDTO() };
        //    _mockService.Setup(service => service.GetAllAsync(default)).ReturnsAsync(permisos);

        //    // Act
        //    var result = await _controller.GetAll();

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<List<PermisoResponseDTO>>(okResult.Value);
        //    Assert.Equal(2, returnValue.Count);
        //}

        //[Fact]
        //public async Task GetById_ReturnsOkResult_WithPermiso()
        //{
        //    // Arrange
        //    var permiso = new PermisoResponseDTO { FuncionalidadId = 1 };
        //    _mockService.Setup(service => service.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(permiso);

        //    // Act
        //    var result = await _controller.GetById(1);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<PermisoResponseDTO>(okResult.Value);
        //    Assert.Equal(1, returnValue.FuncionalidadId);
        //}

        //[Fact]
        //public async Task GetAllByRoleId_ReturnsOkResult_WithListOfPermisos()
        //{
        //    // Arrange
        //    var permisos = new List<PermisoResponseDTO> { new PermisoResponseDTO(), new PermisoResponseDTO() };
        //    _mockService.Setup(service => service.GetAllByRoleIdAsync(It.IsAny<string>(), default)).ReturnsAsync(permisos);

        //    // Act
        //    var result = await _controller.GetAllByRoleId("role1");

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<List<PermisoResponseDTO>>(okResult.Value);
        //    Assert.Equal(2, returnValue.Count);
        //}

        //[Fact]
        //public async Task Add_ReturnsCreatedAtActionResult_WithPermiso()
        //{
        //    // Arrange
        //    var permiso = new PermisoResponseDTO { FuncionalidadId = 1 };
        //    var request = new PermisoRequestDTO { FuncionalidadId = 1 };
        //    _mockService.Setup(service => service.AddAsync(It.IsAny<PermisoRequestDTO>(), default)).ReturnsAsync((true, permiso));

        //    // Act
        //    var result = await _controller.Add(request);

        //    // Assert
        //    var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        //    var returnValue = Assert.IsType<PermisoResponseDTO>(createdAtActionResult.Value);
        //    Assert.Equal(1, returnValue.FuncionalidadId);
        //}

        //[Fact]
        //public async Task Update_ReturnsNoContentResult()
        //{
        //    // Arrange
        //    var permiso = new PermisoResponseDTO { FuncionalidadId = 1 };

        //    // Act
        //    var result = await _controller.Update(permiso);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNoContentResult_WhenPermisoExists()
        //{
        //    // Arrange
        //    var permiso = new PermisoResponseDTO { FuncionalidadId = 1 };
        //    _mockService.Setup(service => service.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(permiso);
        //    _mockService.Setup(service => service.DeleteAsync(It.IsAny<PermisoResponseDTO>(), default)).Returns(Task.FromResult(true));

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNotFoundResult_WhenPermisoDoesNotExist()
        //{
        //    // Arrange
        //    _mockService.Setup(service => service.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync((PermisoResponseDTO)null);

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NotFoundResult>(result);
        //}
    }
}