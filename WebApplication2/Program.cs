using WebApplication2.Services;
using WebApplication2.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductWarehouseService, ProductWarehouseService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();