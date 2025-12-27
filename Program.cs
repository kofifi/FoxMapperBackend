using FoxMapperBackend.Data;
using FoxMapperBackend.Models;
using FoxMapperBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ====== CORS dla frontu (Vite React na http://localhost:5173) ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy
            // Dev: pozwól też na hosty typu http://192.168.x.x:5173 (telefon w tej samej sieci)
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ====== DB CONTEXT (SQLite) ======
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ====== COURIER LOCATION STORE (in-memory) ======
builder.Services.AddSingleton<ICourierLocationStore, InMemoryCourierLocationStore>();

// ====== OSRM ROUTING SERVICE ======
builder.Services.AddHttpClient<IRoutingService, OsrmRoutingService>(client =>
{
    // Czytamy z appsettings.json -> "Osrm": { "BaseUrl": "http://localhost:5000" }
    var baseUrl = builder.Configuration["Osrm:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
});

// ====== MVC / SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ====== DB MIGRATIONS + DEV SEED ======
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var hasChanges = false;

    if (!db.Couriers.Any())
    {
        db.Couriers.Add(new Courier
        {
            Code = "C-001",
            FirstName = "Default",
            LastName = "Courier",
            Email = "courier@example.com",
            PhoneNumber = "000-000-000",
            IsActive = true
        });
        hasChanges = true;
    }

    if (!db.Depots.Any())
    {
        db.Depots.Add(new Depot
        {
            Name = "Main Depot",
            AddressLine = "Default Depot",
            City = "Warsaw",
            PostalCode = "00-001",
            Lat = 52.2297,
            Lng = 21.0122
        });
        hasChanges = true;
    }

    if (hasChanges)
    {
        db.SaveChanges();
    }
}

// ====== PIPELINE ======
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// CORS musi być PRZED MapControllers / handlerami endpointów
app.UseCors("FrontendCors");

app.UseAuthorization();

app.MapControllers();

app.Run();
