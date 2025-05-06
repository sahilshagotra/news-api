using NewsApi.Models;
using NewsApi.Services.Interfaces;
using NewsApi.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient<INewsService, NewsService>()
    .ConfigureHttpClient(client =>
    {
        var hackerNewsUrl = builder.Configuration.GetValue<string>("NewsBaseUrl");
        client.BaseAddress = new Uri(hackerNewsUrl);
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Register Memorycache service
builder.Services.AddMemoryCache();

// Register Http client
builder.Services.AddHttpClient();
builder.Services.AddScoped<INewsService, NewsService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

