using Microsoft.AspNetCore.Mvc;
using Moq;
using RescueFlow.Controllers;
using RescueFlow.DTO.Area.Request;
using RescueFlow.DTO.Area.Response;
using RescueFlow.Interfaces;
using Xunit;

namespace RescueFlow.Tests.Controllers
{
    public class AreasControllerTests
    {
        private readonly AreasController _controller;
        private readonly Mock<IAreaService> _mockService;

        public AreasControllerTests()
        {
            _mockService = new Mock<IAreaService>();
            _controller = new AreasController(_mockService.Object);
        }

        [Fact]
        public async Task AddArea_ReturnsOk_WithCorrectResources()
        {
            var request = new AddAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = 5,
                RequiredResources = new Dictionary<string, int>
                {
                    { "Medicine", 300 },
                    { "Food", 400 }
                },
                TimeConstraintHours = 24
            };

            var expected = new AddAreaResponse
            {
                AreaId = "A1",
                UrgencyLevel = 5,
                RequiredResources = request.RequiredResources,
                TimeConstraintHours = 24
            };

            _mockService.Setup(s => s.AddArea(request)).ReturnsAsync(expected);

            var result = await _controller.AddArea(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<AddAreaResponse>(okResult.Value);

            Assert.Equal("A1", actual.AreaId);
            Assert.Equal(2, actual.RequiredResources.Count);
            Assert.Equal(300, actual.RequiredResources["Medicine"]);
            Assert.Equal(400, actual.RequiredResources["Food"]);
        }

        [Fact]
        public async Task GetAreas_ReturnsOkResult_WithList()
        {
            var list = new List<GetAreaResponse>
            {
                new GetAreaResponse
                {
                    AreaId = "A1",
                    UrgencyLevel = 3,
                    RequiredResources = new Dictionary<string, int> { { "Water", 10 } },
                    TimeConstraintHours = 6
                }
            };

            _mockService.Setup(s => s.GetAreas()).ReturnsAsync(list);

            var result = await _controller.GetAreas();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<List<GetAreaResponse>>(okResult.Value);
            Assert.Single(actual);
        }

        [Fact]
        public async Task GetAreaById_ReturnsOk_WhenFound()
        {
            var expected = new GetAreaResponse
            {
                AreaId = "A2",
                UrgencyLevel = 2,
                RequiredResources = new Dictionary<string, int> { { "Water", 5 } },
                TimeConstraintHours = 3
            };

            _mockService.Setup(s => s.GetAreasById("A2")).ReturnsAsync(expected);

            var result = await _controller.GetAreaById("A2");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<GetAreaResponse>(okResult.Value);
            Assert.Equal("A2", actual.AreaId);
        }

        [Fact]
        public async Task DeleteArea_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAreaById("A1")).Returns(Task.CompletedTask);

            var result = await _controller.DeleteArea("A1");

            Assert.IsType<NoContentResult>(result);
        }
    }
}
