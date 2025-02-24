var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Enable CORS to allow requests from Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy => policy.WithOrigins("http://localhost:5069")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Apply the CORS policy
app.UseCors("AllowBlazorApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
