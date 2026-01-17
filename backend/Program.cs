using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using UglyToad.PdfPig;
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

app.MapPost("/api/cv/check", async (HttpRequest request, IChatCompletionService chatService, CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Expected multipart form data." });
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var textInput = form["text"].ToString();
    var file = form.Files.FirstOrDefault();
    var extractedText = string.Empty;
    OcrResult? ocrResult = null;

    if (file is { Length: > 0 })
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        await using var stream = file.OpenReadStream();
        try
        {
            extractedText = extension switch
            {
                ".pdf" => ExtractPdfText(stream),
                ".docx" => ExtractDocxText(stream),
                _ => string.Empty
            };
        }
        catch (Exception)
        {
            extractedText = string.Empty;
        }

        if (extension == ".pdf" && string.IsNullOrWhiteSpace(extractedText))
        {
            ocrResult = await TryExtractPdfTextWithOcrAsync(stream, cancellationToken);
            extractedText = ocrResult.Text;
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            var error = extension switch
            {
                ".pdf" => ocrResult?.Error ?? "Unable to extract text from the uploaded PDF.",
                ".docx" => "Unable to extract text from the uploaded DOCX.",
                _ => "Unsupported file type. Upload a PDF or DOCX."
            };

            return Results.BadRequest(new { error });
        }
    }

    var combined = string.Join("\n\n", textInput, extractedText).Trim();
    if (string.IsNullOrWhiteSpace(combined))
    {
        return Results.BadRequest(new { error = "Provide resume text or a PDF/DOCX file." });
    }

    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage(
        "You are a CV reviewer. Return ONLY a JSON object with keys: summary (string), score (0-100 integer), suggestions (array of strings). " +
        "Use a balanced rubric: ATS, grammar, and role fit. Keep suggestions concise.");
    chatHistory.AddUserMessage($"Resume content:\n{combined}");

    var settings = new OpenAIPromptExecutionSettings
    {
        Temperature = 0.2,
        MaxTokens = 512
    };

    var response = await chatService.GetChatMessageContentAsync(
        chatHistory,
        settings,
        cancellationToken: cancellationToken);

    var content = response.Content ?? string.Empty;
    if (!TryParseCvReview(content, out var review))
    {
        review = new CvReview(content.Trim(), 0, Array.Empty<string>());
    }

    review = review with { Score = Math.Clamp(review.Score, 0, 100) };
    return Results.Ok(review);
})
.WithName("CheckCv");

app.MapGet("/api/hello", () =>
{
    return Results.Ok(new { message = "Hello from the API." });
})
.WithName("Hello");

app.Run();

static bool TryParseCvReview(string content, out CvReview review)
{
    review = new CvReview(string.Empty, 0, Array.Empty<string>());
    if (string.IsNullOrWhiteSpace(content))
    {
        return false;
    }

    var json = ExtractJsonObject(content);
    if (string.IsNullOrWhiteSpace(json))
    {
        return false;
    }

    try
    {
        var parsed = JsonSerializer.Deserialize<CvReview>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed is null)
        {
            return false;
        }

        review = parsed with { Suggestions = parsed.Suggestions ?? Array.Empty<string>() };
        return true;
    }
    catch (JsonException)
    {
        return false;
    }
}

static string ExtractJsonObject(string content)
{
    var trimmed = content.Trim();
    if (trimmed.StartsWith("```", StringComparison.Ordinal))
    {
        trimmed = trimmed.Trim('`').Trim();
        if (trimmed.StartsWith("json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[4..].Trim();
        }
    }

    var start = trimmed.IndexOf('{');
    var end = trimmed.LastIndexOf('}');
    if (start < 0 || end <= start)
    {
        return string.Empty;
    }

    return trimmed[start..(end + 1)].Trim();
}

static string ExtractPdfText(Stream stream)
{
    var builder = new StringBuilder();
    if (stream.CanSeek)
    {
        stream.Position = 0;
    }
    using var document = PdfDocument.Open(stream);
    foreach (var page in document.GetPages())
    {
        builder.AppendLine(page.Text);
    }
    return builder.ToString();
}

static string ExtractDocxText(Stream stream)
{
    using var doc = WordprocessingDocument.Open(stream, false);
    return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
}

static async Task<OcrResult> TryExtractPdfTextWithOcrAsync(Stream stream, CancellationToken cancellationToken)
{
    try
    {
        var inputPath = Path.Combine(Path.GetTempPath(), $"cv-input-{Guid.NewGuid():N}.pdf");
        var outputPath = Path.Combine(Path.GetTempPath(), $"cv-output-{Guid.NewGuid():N}.pdf");
        var sidecarPath = Path.Combine(Path.GetTempPath(), $"cv-output-{Guid.NewGuid():N}.txt");

        await using (var fileStream = File.Create(inputPath))
        {
            stream.Position = 0;
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "ocrmypdf",
            Arguments = $"--skip-text --sidecar \"{sidecarPath}\" \"{inputPath}\" \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return new OcrResult(string.Empty, "OCR tool not available. Install ocrmypdf and tesseract.");
        }

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0 || !File.Exists(sidecarPath))
        {
            var errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            var message = string.IsNullOrWhiteSpace(errorOutput)
                ? "OCR failed to extract text from the PDF."
                : $"OCR failed: {errorOutput.Trim()}";
            return new OcrResult(string.Empty, message);
        }

        var text = await File.ReadAllTextAsync(sidecarPath, cancellationToken);
        return new OcrResult(text, string.Empty);
    }
    catch (FileNotFoundException)
    {
        return new OcrResult(string.Empty, "OCR tool not available. Install ocrmypdf and tesseract.");
    }
    catch (Exception)
    {
        return new OcrResult(string.Empty, "OCR failed to extract text from the PDF.");
    }
    finally
    {
        TryDeleteTempFiles("cv-input-", ".pdf");
        TryDeleteTempFiles("cv-output-", ".pdf");
        TryDeleteTempFiles("cv-output-", ".txt");
    }
}

static void TryDeleteTempFiles(string prefix, string extension)
{
    try
    {
        var tempDir = Path.GetTempPath();
        foreach (var file in Directory.EnumerateFiles(tempDir, $"{prefix}*{extension}"))
        {
            File.Delete(file);
        }
    }
    catch (Exception)
    {
        // Best-effort cleanup.
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record ChatRequest(string Message, double? Temperature, int? MaxTokens);
record ChatResponse(string Message);
record CvReview(string Summary, int Score, string[] Suggestions);
record OcrResult(string Text, string Error);
