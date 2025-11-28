using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Data;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQLite
builder.Services.AddDbContext<PizzaDb>(options =>
    options.UseSqlite("Data Source=pizzas.db")
);

// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});

var app = builder.Build();

// Ensure SQLite database and tables are created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaDb>();
    db.Database.EnsureCreated();
}

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
    });
}

// Test endpoint
app.MapGet("/", () => "Hello World!");

// -----------------------
// CRUD Endpoints
// -----------------------

// List all pizzas
app.MapGet("/pizzas", async (PizzaDb db) =>
    await db.Pizzas.ToListAsync()
);

// Get pizza by ID
app.MapGet("/pizza/{id}", async (int id, PizzaDb db) =>
    await db.Pizzas.FindAsync(id) is Pizza pizza
        ? Results.Ok(pizza)
        : Results.NotFound()
);

// Create pizza
app.MapPost("/pizza", async (Pizza pizza, PizzaDb db) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

// Update pizza
app.MapPut("/pizza/{id}", async (int id, Pizza updatepizza, PizzaDb db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();

    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete pizza
app.MapDelete("/pizza/{id}", async (int id, PizzaDb db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();

    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();
