namespace RescueFlow.Interfaces
{
    public interface IRedisCacheService
    {
        // บันทึกข้อมูลลงใน Redis
        Task SetCacheAsync(string key, string value, TimeSpan expiry);
        // ดึงข้อมูลจาก Redis
        Task<string?> GetCacheAsync(string key);
        // ลบข้อมูลจาก Redis
        Task DeleteCacheAsync(string key);
    }
}
