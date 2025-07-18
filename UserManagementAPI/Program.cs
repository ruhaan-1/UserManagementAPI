using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

// Register the Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

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
    var user = users.Find(u => u.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// POST: Create user
app.MapPost("/api/users", ([FromBody] User newUser) =>
{
    try
    {
        // Validate input
        var context = new ValidationContext(newUser);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(newUser, context, results, true))
        {
            return Results.BadRequest(results);
        }

        // Check for duplicate email
        if (users.Any(u => u.Email == newUser.Email))
        {
            return Results.BadRequest("Email already exists.");
        }

        // Assign ID and add user
        var maxId = users.Any() ? users.Max(u => u.Id) : 0;
        newUser.Id = maxId + 1;
        users.Add(newUser);

        return Results.Created($"/api/users/{newUser.Id}", newUser);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error creating user: {ex.Message}");
    }
});

// PUT: Update user
app.MapPut("/api/users/{id}", (int id, [FromBody] User updatedUser) =>
{
    try
    {
        var context = new ValidationContext(updatedUser);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(updatedUser, context, results, true))
        {
            return Results.BadRequest(results);
        }

        var user = users.Find(u => u.Id == id);
        if (user is null) return Results.NotFound();

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        user.Age = updatedUser.Age;

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error updating user: {ex.Message}");
    }
});

// DELETE: Remove user
app.MapDelete("/api/users/{id}", (int id) =>
{
    try
    {
        var user = users.Find(u => u.Id == id);
        if (user is null) return Results.NotFound();

        users.Remove(user);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error deleting user: {ex.Message}");
    }
});

app.Run();
