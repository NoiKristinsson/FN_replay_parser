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
// Force the API to use port 5056
app.Urls.Add("http://0.0.0.0:5056");


// Apply the CORS policy
app.UseCors("AllowBlazorApp");
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
app.Run();
