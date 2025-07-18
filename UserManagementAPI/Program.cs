using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sample in-memory data
var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com", Age = 30 },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com", Age = 25 }
};

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// GET: All users
app.MapGet("/api/users", () => Results.Ok(users));

// GET: User by ID
app.MapGet("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// POST: Create user
app.MapPost("/api/users", (User newUser) =>
{
    newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
    users.Add(newUser);
    return Results.Created($"/api/users/{newUser.Id}", newUser);
});

// PUT: Update user
app.MapPut("/api/users/{id}", (int id, User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    user.Age = updatedUser.Age;

    return Results.NoContent();
});

// DELETE: Remove user
app.MapDelete("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.Run();