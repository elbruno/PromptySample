#pragma warning disable SKEXP0040

using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];

// Create a chat completion service
builder.Services.AddKernel();
builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async (HttpContext context, Kernel kernel) =>
{
    var forecast = new List<WeatherForecast>();
    for (int i = 0; i < 3; i++)
    {
        var forecastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
        var forecastTemperature = Random.Shared.Next(-20, 55);

        var weatherFunc = kernel.CreateFunctionFromPromptyFile("weatherforecastdesc.prompty");
        var forecastSummary = await weatherFunc.InvokeAsync<string>(kernel, new()
        {
            { "today", $"{DateOnly.FromDateTime(DateTime.Now)}" },
            { "date", $"{forecastDate}" },
            { "forecastTemperatureC", $"{forecastTemperature}" }
        });

        forecast.Add(new WeatherForecast(forecastDate, forecastTemperature, forecastSummary));
    }
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}