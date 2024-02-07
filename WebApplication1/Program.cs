using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
    options.InstanceName = "SampleInstance";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Get value from Redis cache
app.MapGet("/debug", async (IDistributedCache cache) =>
    {
        const string cacheKey = "test";
        const string appVersion = "V2";
        
        var value = await cache.GetStringAsync(cacheKey);

        // ReSharper disable once InvertIf
        if (value is null)
        {
            value = appVersion;
            await cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(100)
            });
        }
        
        return $"App Version: {appVersion}, Cache: {value}";
    })
    .WithName("Debug")
    .WithOpenApi();

app.Run();
