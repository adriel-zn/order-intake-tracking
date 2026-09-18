using Microsoft.EntityFrameworkCore;
using OrderIntake.Api.Validation;
using OrderIntake.Application.Orders;
using OrderIntake.Domain.Orders;
using OrderIntake.Infrastructure.Persistence;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.Filters.Add<DomainValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Order Intake API", Version = "v1" });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});
builder.Services.AddDbContext<OrderIntakeDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("OrderIntake")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:OrderIntake for SQL Server.")));
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
const string angularDevCorsPolicy = "AngularDev";
builder.Services.AddCors(options => options.AddPolicy(angularDevCorsPolicy, policy =>
    policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Intake API v1"));
}
app.UseCors(angularDevCorsPolicy);
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "OrderIntake.Api", status = "running" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();
public partial class Program { }
