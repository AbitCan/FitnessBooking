using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// DI registrations
builder.Services.AddSingleton<IMemberRepository, InMemoryMemberRepository>();
builder.Services.AddSingleton<IClassRepository, InMemoryClassRepository>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<ReservationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// IMPORTANT: map controllers
app.MapControllers();

app.Run();

// For IntegrationTests (WebApplicationFactory)
public partial class Program { }
