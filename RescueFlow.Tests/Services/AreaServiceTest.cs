using Azure.Core;
using Moq;
using RescueFlow.DTO.Area.Request;
using RescueFlow.Interfaces;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;
using RescueFlow.Services;
using Xunit;

namespace RescueFlow.Tests.Services
{
    public class AreaServiceTests
    {
        private readonly AreaService _areaService;
        private readonly Mock<IAreaRepository> _mockRepo;
        private readonly Mock<IRedisCacheService> _redisCacheMock = new();

        public AreaServiceTests()
        {
            _mockRepo = new Mock<IAreaRepository>();
            _areaService = new AreaService(_mockRepo.Object, _redisCacheMock.Object);
        }

        [Theory]
        [InlineData(null, 3, 5, "AreaId ต้องมีข้อมูล")]
        [InlineData("A1", 0, 5, "UrgencyLevel ต้องอยู่ระหว่าง 1 ถึง 5")]
        [InlineData("A1", 3, 5, "RequiredResources ต้องมีข้อมูล")]
        [InlineData("A1", 3, 0, "TimeConstraintHours ต้องมากว่า 0")]
        public async Task AddArea_ShouldThrowValidationException_WhenInputInvalid(string areaId, int urgencyLevel, int timeConstraintHours, string expectedMessage)
        {
            var request = new AddAreaRequest
            {
                AreaId = areaId,
                UrgencyLevel = urgencyLevel,
                RequiredResources = expectedMessage == "RequiredResources ต้องมีข้อมูล" ? null : new Dictionary<string, int> { { "Water", 100 } },
                TimeConstraintHours = timeConstraintHours
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _areaService.AddArea(request));

            Assert.Equal(expectedMessage, exception.Message);
        }
        [Fact]
        public async Task AddArea_ShouldThrow_WhenAreaAlreadyExists()
        {
            var request = new AddAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = 3,
                RequiredResources = new Dictionary<string, int> { { "Food", 5 } },
                TimeConstraintHours = 6
            };

            _mockRepo.Setup(r => r.ExistsAsync("A1")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>_areaService.AddArea(request));

            Assert.Equal("ข้อมูลของ AreaId 'A1' มีอยู่แล้ว", ex.Message);
        }

        [Fact]
        public async Task AddArea_ShouldReturnCorrectData_WhenValid()
        {
            var request = new AddAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = 3,
                RequiredResources = new Dictionary<string, int> { { "Water", 10 }, { "Food", 5 } },
                TimeConstraintHours = 12
            };

            _mockRepo.Setup(r => r.ExistsAsync(request.AreaId)).ReturnsAsync(false);

            var result = await _areaService.AddArea(request);

            Assert.Equal("A1", result.AreaId);
            Assert.Equal(3, result.UrgencyLevel);
            Assert.Equal(2, result.RequiredResources.Count);
            Assert.Equal(10, result.RequiredResources["Water"]);
        }

        [Fact]
        public async Task AddArea_ShouldThrow_WhenAreaIdExists()
        {
            var request = new AddAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = 3,
                RequiredResources = new Dictionary<string, int> { { "Water", 5 } },
                TimeConstraintHours = 6
            };

            _mockRepo.Setup(r => r.ExistsAsync("A1")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _areaService.AddArea(request));
        }

        [Fact]
        public async Task GetAreaById_ShouldThrow_WhenAreaIdIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _areaService.GetAreasById(null));

            Assert.Equal("AreaId ต้องมีข้อมูล", exception.Message);
        }

        [Fact]
        public async Task GetAreaById_ShouldThrow_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync("A2")).ReturnsAsync((Area?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _areaService.GetAreasById("A2"));

            Assert.Equal($"ไม่พบข้อมูล AreaId 'A2'", exception.Message);
        }

        [Theory]
        [InlineData(null, 3, 5, "AreaId ต้องมีข้อมูล และต้องตรงกัน")]
        [InlineData("A2", 3, 5, "AreaId ต้องมีข้อมูล และต้องตรงกัน")]
        [InlineData("A1", 0, 5, "UrgencyLevel ต้องอยู่ระหว่าง 1 ถึง 5")]
        [InlineData("A1", 3, 5, "RequiredResources ต้องมีข้อมูล")]
        [InlineData("A1", 3, 0, "TimeConstraintHours ต้องมากว่า 0")]
        public async Task UpdateArea_ShouldThrowValidationException_WhenInputInvalid(string areaId,int urgencyLevel, int timeConstraintHours,string expectedMessage)
        {
            var request = new UpdateAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = urgencyLevel,
                RequiredResources = expectedMessage == "RequiredResources ต้องมีข้อมูล" ? null : new Dictionary<string, int> { { "Water", 100 } },
                TimeConstraintHours = timeConstraintHours
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _areaService.UpdateArea(request, areaId));

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task UpdateArea_ShouldUpdateSuccessfully_WhenValid()
        {
            var area = new Area
            {
                AreaId = "A1",
                UrgencyLevel = 2,
                RequiredResources = new Dictionary<string, int> { { "Food", 2 } },
                TimeConstraintHours = 8
            };

            _mockRepo.Setup(r => r.GetByIdAsync("A1")).ReturnsAsync(area);

            var request = new UpdateAreaRequest
            {
                AreaId = "A1",
                UrgencyLevel = 5,
                RequiredResources = new Dictionary<string, int> { { "Water", 10 } },
                TimeConstraintHours = 4
            };

            var result = await _areaService.UpdateArea(request, "A1");

            Assert.Equal("A1", result.AreaId);
            Assert.Equal(5, result.UrgencyLevel);
            Assert.Equal(4, result.TimeConstraintHours);
            Assert.True(result.RequiredResources.ContainsKey("Water"));
        }

        [Fact]
        public async Task DeleteAreaById_ShouldThrow_WhenAreaIdIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _areaService.DeleteAreaById(null));

            Assert.Equal("AreaId ต้องมีข้อมูล", exception.Message);
        }


        [Fact]
        public async Task DeleteAreaById_ShouldThrow_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync("A3")).ReturnsAsync((Area?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _areaService.DeleteAreaById("A3"));
        }

    }
}
