using IPLStore.Application.Interfaces;
using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Interfaces.Service;
using IPLStore.Application.Services;
using IPLStore.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddControllers();

builder.Services.AddDbContext<IPLStoreDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IPLStoreRepository>();
builder.Services.AddScoped<ICartRepository>(sp => sp.GetRequiredService<IPLStoreRepository>());
builder.Services.AddScoped<IOrderRepository>(sp => sp.GetRequiredService<IPLStoreRepository>());
builder.Services.AddScoped<IProductQueries>(sp => sp.GetRequiredService<IPLStoreRepository>());
builder.Services.AddScoped<IOrderQueries>(sp => sp.GetRequiredService<IPLStoreRepository>());

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
