using RescueFlow.Interfaces;
using StackExchange.Redis;

namespace RescueFlow.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer connection)
        {
            _connection = connection;
            _database = _connection.GetDatabase();
        }

        public async Task SetCacheAsync(string key, string value, TimeSpan expiry)
        {
            try
            {
                await _database.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ไม่สามารถบันทึกข้อมูลใน Redis", ex);
            }
        }

        public async Task<string?> GetCacheAsync(string key)
        {
            try
            {
                return await _database.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ไม่สามารถดึงข้อมูลจาก Redis ได้: {ex.Message}", ex);
            }
        }

        public async Task DeleteCacheAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ไม่สามารถลบข้อมูลจาก Redis ได้: {ex.Message}", ex);
            }
        }
    }

}
