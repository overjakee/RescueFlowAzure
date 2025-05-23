using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Interfaces;
using RescueFlow.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ลงทะเบียน DbContext
builder.Services.AddDbContext<RescueFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
// เพิ่มการเชื่อมต่อ Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection"))
);

// Register Services กับ Interface
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
