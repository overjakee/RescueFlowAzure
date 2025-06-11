using RescueFlow.Data;
using RescueFlow.DTO.Truck.Request;
using RescueFlow.DTO.Truck.Response;
using RescueFlow.Interfaces;
using Microsoft.EntityFrameworkCore;
using RescueFlow.Models;

namespace RescueFlow.Services
{
    public class TruckService : ITruckService
    {
        private readonly RescueFlowDbContext _context;
        public TruckService(RescueFlowDbContext context)
        {
            _context = context;
        }

        public async Task<AddTruckResponse> AddTruck(AddTruckRequest request)
        {
            ValidateAddTruckRequest(request);

            var exists = await _context.Trucks.FirstOrDefaultAsync(a => a.TruckId == request.TruckId);
            if (exists != null)
                throw new InvalidOperationException($"ข้อมูลของ TruckId '{request.TruckId}' มีอยู่แล้ว");

            var Truck = new Truck
            {
                TruckId = request.TruckId,
                AvailableResources = request.AvailableResources,
                TravelTimeToArea = request.TravelTimeToArea
            };

            _context.Trucks.Add(Truck);
            await _context.SaveChangesAsync();

            var response = new AddTruckResponse
            {
                TruckId = Truck.TruckId,
                AvailableResources = Truck.AvailableResources,
                TravelTimeToArea = Truck.TravelTimeToArea
            };
            return response;
        }

        public async Task<List<GetTruckResponse>> GetTrucks()
        {
            var Trucks = await _context.Trucks.ToListAsync();

            var response = Trucks.Select(a => new GetTruckResponse
            {
                TruckId = a.TruckId,
                AvailableResources = a.AvailableResources,
                TravelTimeToArea = a.TravelTimeToArea
            }).ToList();

            return response;
        }

        public async Task<GetTruckResponse> GetTruckById(string TruckId)
        {
            if (string.IsNullOrEmpty(TruckId))
                throw new ArgumentException("TruckId ต้องมีข้อมูล");

            var Truck = await _context.Trucks.FirstOrDefaultAsync(a => a.TruckId == TruckId);
            if (Truck == null)
                throw new KeyNotFoundException($"ไม่พบข้อมูล TruckId '{TruckId}'");

            var response = new GetTruckResponse
            {
                TruckId = Truck.TruckId,
                AvailableResources = Truck.AvailableResources,
                TravelTimeToArea = Truck.TravelTimeToArea
            };

            return response;
        }

        public async Task<UpdateTruckResponse> UpdateTruck(UpdateTruckRequest request, string TruckId)
        {
            ValidateUpdateTruckRequest(request, TruckId);

            var existingTruck = await _context.Trucks.FirstOrDefaultAsync(a => a.TruckId == request.TruckId);
            if (existingTruck == null)
                throw new InvalidOperationException($"ไม่พบข้อมูล TruckId '{request.TruckId}' ในระบบ");

            existingTruck.AvailableResources = request.AvailableResources;
            existingTruck.TravelTimeToArea = request.TravelTimeToArea;

            await _context.SaveChangesAsync();

            var response = new UpdateTruckResponse
            {
                TruckId = existingTruck.TruckId,
                AvailableResources = existingTruck.AvailableResources,
                TravelTimeToArea = existingTruck.TravelTimeToArea
            };

            return response;
        }

        public async Task DeleteTruckById(string TruckId)
        {
            if (string.IsNullOrEmpty(TruckId))
                throw new ArgumentException("TruckId ต้องมีข้อมูล");

            var Truck = await _context.Trucks.FindAsync(TruckId);
            if (Truck == null)
                throw new KeyNotFoundException($"ไม่พบ Truck ที่มี ID '{TruckId}'");

            _context.Trucks.Remove(Truck);
            await _context.SaveChangesAsync();
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

        private void ValidateUpdateTruckRequest(UpdateTruckRequest request, string TruckId)
        {
            if (string.IsNullOrEmpty(request.TruckId) || string.IsNullOrEmpty(TruckId) || request.TruckId != TruckId)
                throw new ArgumentException("TruckId ต้องมีข้อมูล และต้องตรงกัน");

            if (request.AvailableResources == null || !request.AvailableResources.Any())
                throw new ArgumentException("AvailableResources ต้องมีข้อมูล");

            if (request.TravelTimeToArea == null || !request.TravelTimeToArea.Any())
                throw new ArgumentException("TravelTimeToArea ต้องมีข้อมูล");
        }
    }
}
