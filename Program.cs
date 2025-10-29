using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyApiApp.Data;
using MyApiApp.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1️⃣ Configure Services
// ----------------------------------------------------

// Register EF Core DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Swagger/OpenAPI (via Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApiApp", Version = "v1" });
});

var app = builder.Build();

// ----------------------------------------------------
// 2️⃣ Configure Middleware
// ----------------------------------------------------

// Auto-apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Enable Swagger UI only in development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyApiApp v1");
    c.RoutePrefix = string.Empty; // serve UI at root "/"
});

app.UseHttpsRedirection();

// ----------------------------------------------------
// 3️⃣ Minimal API Endpoints
// ----------------------------------------------------

app.MapGet("/", () => Results.Ok("✅ MyApiApp is running"));

// GET – list all products
app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.ToListAsync())
   .WithName("GetProducts")
   .WithOpenApi();

// GET – Single product by ID
app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.Where(x => x.Id == id).ToListAsync())
   .WithName("GetProductById")
   .WithOpenApi();

// POST – create a new product
app.MapPost("/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithOpenApi();

// ----------------------------------------------------
// 4️⃣ Run the App
// ----------------------------------------------------
app.Run();
