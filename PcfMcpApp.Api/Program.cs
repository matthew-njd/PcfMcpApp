using Microsoft.EntityFrameworkCore;
using PcfMcpApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    //SQLite for dev
    builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseSqlite(connectionString));
}
else
{
    //SQL Server for prod
    builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseSqlServer(connectionString));
}

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddCors(options => options.AddDefaultPolicy(p => 
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

app.MapMcp("/mcp");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (db.Database.EnsureCreated())
    {
        // --- Customers ---
        var customers = new List<Customer>
        {
            new(1, "Acme Corp",          "contact@acme.com"),
            new(2, "Globex Industries",  "sales@globex.com"),
            new(3, "Initech Solutions",  "info@initech.com"),
            new(4, "Umbrella Ltd",       "accounts@umbrella.com"),
            new(5, "Stark Enterprises",  "billing@stark.com"),
            new(6, "Wayne Technologies", "finance@wayne.com"),
        };
        db.Customers.AddRange(customers);

        // --- Sales ---
        var sales = new List<Sale>
        {
            // Acme Corp (Id: 1) — steady mid-range buyer
            new(1,  1,  1200.00m, DateTime.Now.AddMonths(-11)),
            new(2,  1,   850.50m, DateTime.Now.AddMonths(-10)),
            new(3,  1,  3200.00m, DateTime.Now.AddMonths(-9)),
            new(4,  1,   450.00m, DateTime.Now.AddMonths(-7)),
            new(5,  1,  1750.75m, DateTime.Now.AddMonths(-6)),
            new(6,  1,   990.00m, DateTime.Now.AddMonths(-5)),
            new(7,  1,  2100.00m, DateTime.Now.AddMonths(-4)),
            new(8,  1,   375.00m, DateTime.Now.AddMonths(-3)),
            new(9,  1,  1540.00m, DateTime.Now.AddMonths(-2)),
            new(10, 1,   620.00m, DateTime.Now.AddDays(-15)),
            new(11, 1,  2850.00m, DateTime.Now.AddDays(-3)),

            // Globex Industries (Id: 2) — high-value, infrequent
            new(12, 2, 12500.00m, DateTime.Now.AddMonths(-11)),
            new(13, 2,  8750.00m, DateTime.Now.AddMonths(-9)),
            new(14, 2, 15000.00m, DateTime.Now.AddMonths(-8)),
            new(15, 2,  4200.00m, DateTime.Now.AddMonths(-7)),
            new(16, 2, 11300.00m, DateTime.Now.AddMonths(-5)),
            new(17, 2,  9800.00m, DateTime.Now.AddMonths(-4)),
            new(18, 2,  6600.00m, DateTime.Now.AddMonths(-3)),
            new(19, 2, 13400.00m, DateTime.Now.AddMonths(-2)),
            new(20, 2,  7250.00m, DateTime.Now.AddMonths(-1)),
            new(21, 2, 18000.00m, DateTime.Now.AddDays(-10)),

            // Initech Solutions (Id: 3) — frequent small purchases
            new(22, 3,   199.99m, DateTime.Now.AddMonths(-11)),
            new(23, 3,   250.00m, DateTime.Now.AddMonths(-10)),
            new(24, 3,   175.50m, DateTime.Now.AddMonths(-9)),
            new(25, 3,   320.00m, DateTime.Now.AddMonths(-8)),
            new(26, 3,   210.00m, DateTime.Now.AddMonths(-7)),
            new(27, 3,   289.99m, DateTime.Now.AddMonths(-6)),
            new(28, 3,   145.00m, DateTime.Now.AddMonths(-5)),
            new(29, 3,   399.00m, DateTime.Now.AddMonths(-4)),
            new(30, 3,   220.00m, DateTime.Now.AddMonths(-3)),
            new(31, 3,   310.00m, DateTime.Now.AddMonths(-2)),
            new(32, 3,   265.00m, DateTime.Now.AddDays(-20)),
            new(33, 3,   189.00m, DateTime.Now.AddDays(-5)),

            // Umbrella Ltd (Id: 4) — inconsistent, trailing off
            new(34, 4,  5500.00m, DateTime.Now.AddMonths(-11)),
            new(35, 4,  4800.00m, DateTime.Now.AddMonths(-10)),
            new(36, 4,  6200.00m, DateTime.Now.AddMonths(-9)),
            new(37, 4,  3100.00m, DateTime.Now.AddMonths(-8)),
            new(38, 4,  2200.00m, DateTime.Now.AddMonths(-7)),
            new(39, 4,  4100.00m, DateTime.Now.AddMonths(-5)),
            new(40, 4,  1800.00m, DateTime.Now.AddMonths(-4)),
            new(41, 4,   950.00m, DateTime.Now.AddMonths(-3)),
            new(42, 4,   400.00m, DateTime.Now.AddMonths(-2)),
            new(43, 4,   200.00m, DateTime.Now.AddMonths(-1)),

            // Stark Enterprises (Id: 5) — growing rapidly recently
            new(44, 5,   500.00m, DateTime.Now.AddMonths(-11)),
            new(45, 5,   750.00m, DateTime.Now.AddMonths(-10)),
            new(46, 5,  1100.00m, DateTime.Now.AddMonths(-9)),
            new(47, 5,  1400.00m, DateTime.Now.AddMonths(-8)),
            new(48, 5,  2000.00m, DateTime.Now.AddMonths(-7)),
            new(49, 5,  2800.00m, DateTime.Now.AddMonths(-6)),
            new(50, 5,  3500.00m, DateTime.Now.AddMonths(-5)),
            new(51, 5,  4200.00m, DateTime.Now.AddMonths(-4)),
            new(52, 5,  5100.00m, DateTime.Now.AddMonths(-3)),
            new(53, 5,  6800.00m, DateTime.Now.AddMonths(-2)),
            new(54, 5,  8500.00m, DateTime.Now.AddMonths(-1)),
            new(55, 5, 11000.00m, DateTime.Now.AddDays(-7)),

            // Wayne Technologies (Id: 6) — large quarterly spikes
            new(56, 6,  3000.00m, DateTime.Now.AddMonths(-11)),
            new(57, 6, 22000.00m, DateTime.Now.AddMonths(-10)),
            new(58, 6,  2500.00m, DateTime.Now.AddMonths(-9)),
            new(59, 6,  2800.00m, DateTime.Now.AddMonths(-8)),
            new(60, 6, 19500.00m, DateTime.Now.AddMonths(-7)),
            new(61, 6,  3100.00m, DateTime.Now.AddMonths(-6)),
            new(62, 6,  2700.00m, DateTime.Now.AddMonths(-5)),
            new(63, 6, 24000.00m, DateTime.Now.AddMonths(-4)),
            new(64, 6,  3300.00m, DateTime.Now.AddMonths(-3)),
            new(65, 6,  2900.00m, DateTime.Now.AddMonths(-2)),
            new(66, 6, 21000.00m, DateTime.Now.AddMonths(-1)),
            new(67, 6,  3400.00m, DateTime.Now.AddDays(-8)),
        };
        db.Sales.AddRange(sales);

        await db.SaveChangesAsync();
        Console.WriteLine("--> SQLite Database created and seeded with test data.");
    }
}

app.Run();
