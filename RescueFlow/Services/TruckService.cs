using RescueFlow.DTO.Truck.Request;
using RescueFlow.DTO.Truck.Response;
using RescueFlow.Interfaces;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;

namespace RescueFlow.Services
{
    public class TruckService : ITruckService
    {
        private readonly ITruckRepository _truckRepository;

        public TruckService(ITruckRepository truckRepository)
        {
            _truckRepository = truckRepository;
        }

        public async Task<AddTruckResponse> AddTruck(AddTruckRequest request)
        {
            ValidateAddTruckRequest(request);

            if (await _truckRepository.ExistsAsync(request.TruckId))
                throw new InvalidOperationException($"ข้อมูลของ TruckId '{request.TruckId}' มีอยู่แล้ว");

            var truck = new Truck
            {
                TruckId = request.TruckId,
                AvailableResources = request.AvailableResources,
                TravelTimeToArea = request.TravelTimeToArea
            };

            await _truckRepository.AddAsync(truck);

            return new AddTruckResponse
            {
                TruckId = truck.TruckId,
                AvailableResources = truck.AvailableResources,
                TravelTimeToArea = truck.TravelTimeToArea
            };
        }

        public async Task<List<GetTruckResponse>> GetTrucks()
        {
            var trucks = await _truckRepository.GetAllAsync();

            return trucks.Select(t => new GetTruckResponse
            {
                TruckId = t.TruckId,
                AvailableResources = t.AvailableResources,
                TravelTimeToArea = t.TravelTimeToArea
            }).ToList();
        }

        public async Task<GetTruckResponse> GetTruckById(string truckId)
        {
            if (string.IsNullOrEmpty(truckId))
                throw new ArgumentException("TruckId ต้องมีข้อมูล");

            var truck = await _truckRepository.GetByIdAsync(truckId);
            if (truck == null)
                throw new KeyNotFoundException($"ไม่พบข้อมูล TruckId '{truckId}'");

            return new GetTruckResponse
            {
                TruckId = truck.TruckId,
                AvailableResources = truck.AvailableResources,
                TravelTimeToArea = truck.TravelTimeToArea
            };
        }

        public async Task<UpdateTruckResponse> UpdateTruck(UpdateTruckRequest request, string truckId)
        {
            ValidateUpdateTruckRequest(request, truckId);

            var existingTruck = await _truckRepository.GetByIdAsync(truckId);
            if (existingTruck == null)
                throw new InvalidOperationException($"ไม่พบข้อมูล TruckId '{request.TruckId}' ในระบบ");

            existingTruck.AvailableResources = request.AvailableResources;
            existingTruck.TravelTimeToArea = request.TravelTimeToArea;

            await _truckRepository.UpdateAsync(existingTruck);

            return new UpdateTruckResponse
            {
                TruckId = existingTruck.TruckId,
                AvailableResources = existingTruck.AvailableResources,
                TravelTimeToArea = existingTruck.TravelTimeToArea
            };
        }

        public async Task DeleteTruckById(string truckId)
        {
            if (string.IsNullOrEmpty(truckId))
                throw new ArgumentException("TruckId ต้องมีข้อมูล");

            var truck = await _truckRepository.GetByIdAsync(truckId);
            if (truck == null)
                throw new KeyNotFoundException($"ไม่พบ Truck ที่มี ID '{truckId}'");

            await _truckRepository.DeleteAsync(truck);
        }

        private void ValidateAddTruckRequest(AddTruckRequest request)
        {
            if (string.IsNullOrEmpty(request.TruckId))
                throw new ArgumentException("TruckId ต้องมีข้อมูล");

            if (request.AvailableResources == null || !request.AvailableResources.Any())
                throw new ArgumentException("AvailableResources ต้องมีข้อมูล");

            if (request.TravelTimeToArea == null || !request.TravelTimeToArea.Any())
                throw new ArgumentException("TravelTimeToArea ต้องมีข้อมูล");
        }

        private void ValidateUpdateTruckRequest(UpdateTruckRequest request, string truckId)
        {
            if (string.IsNullOrEmpty(request.TruckId) || string.IsNullOrEmpty(truckId) || request.TruckId != truckId)
                throw new ArgumentException("TruckId ต้องมีข้อมูล และต้องตรงกัน");

            if (request.AvailableResources == null || !request.AvailableResources.Any())
                throw new ArgumentException("AvailableResources ต้องมีข้อมูล");

            if (request.TravelTimeToArea == null || !request.TravelTimeToArea.Any())
                throw new ArgumentException("TravelTimeToArea ต้องมีข้อมูล");
        }
    }
}
