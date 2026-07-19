using IPLStore.Application.Interfaces;
using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Interfaces.Service;
using IPLStore.Application.Services;
using IPLStore.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
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
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(error => error.Run(async context =>
{
    context.Response.ContentType = "application/problem+json";
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
    logger.LogError(exceptionFeature?.Error, "Unhandled exception");

    await context.Response.WriteAsJsonAsync(new
    {
        status = 500,
        title = "An unexpected error occurred.",
        detail = app.Environment.IsDevelopment() ? exceptionFeature?.Error?.Message : null
    });
}));

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
