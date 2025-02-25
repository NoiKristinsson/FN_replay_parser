var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Enable CORS to allow requests from Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy.WithOrigins("http://localhost:5069", "https://fn.logn.is")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Apply the CORS policy
app.UseCors("AllowBlazorApp");
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
app.Run();
