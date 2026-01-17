using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434/v1";
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "llama3";

builder.Services.AddSingleton<IChatCompletionService>(_ =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(ollamaBaseUrl) };
    return new OpenAIChatCompletionService(ollamaModel, apiKey: "ollama", httpClient: httpClient);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("client");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/api/chat", async (ChatRequest request, IChatCompletionService chatService, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(request.Message);

    var settings = new OpenAIPromptExecutionSettings
    {
        Temperature = request.Temperature ?? 0.7,
        MaxTokens = request.MaxTokens ?? 256
    };

    var response = await chatService.GetChatMessageContentAsync(
        chatHistory,
        settings,
        cancellationToken: cancellationToken);

    return Results.Ok(new ChatResponse(response.Content ?? string.Empty));
})
.WithName("ChatWithOllama");

app.MapPost("/api/chat/stream", async (HttpContext context, ChatRequest request, IChatCompletionService chatService, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "Message is required." }, cancellationToken);
        return;
    }

    context.Response.Headers.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";

    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(request.Message);

    var settings = new OpenAIPromptExecutionSettings
    {
        Temperature = request.Temperature ?? 0.7,
        MaxTokens = request.MaxTokens ?? 256
    };

    await foreach (var message in chatService.GetStreamingChatMessageContentsAsync(
                       chatHistory,
                       settings,
                       cancellationToken: cancellationToken))
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            continue;
        }

        var payload = JsonSerializer.Serialize(new { delta = message.Content });
        await context.Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }

    await context.Response.WriteAsync("data: {\"done\":true}\n\n", cancellationToken);
    await context.Response.Body.FlushAsync(cancellationToken);
})
.WithName("ChatWithOllamaStream");

app.MapGet("/api/hello", () =>
{
    return Results.Ok(new { message = "Hello from the API." });
})
.WithName("Hello");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record ChatRequest(string Message, double? Temperature, int? MaxTokens);
record ChatResponse(string Message);
