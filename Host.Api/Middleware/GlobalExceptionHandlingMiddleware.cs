namespace Host.Api.Middleware;

using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Shared.Api.Models;

// Simple middleware for global exception handling.
public class GlobalExceptionHandlingMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger, RequestDelegate next)
    {
        this._logger = logger;
        this._next = next;

        this._jsonSerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context is null || this._next is null)
        {
            return;
        }

        try
        {
            await this._next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            this._logger.LogError("Exception was thrown: {Exception}", exception);
            await this.HandleExceptionAsync(context, new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Internal server error"
            }).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, ErrorResponse response)
    {
        string jsonResponse = JsonSerializer.Serialize(response, this._jsonSerializerOptions);
        this._logger.LogDebug("LogError response: {JsonResponse}", jsonResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;

        await context.Response.WriteAsync(jsonResponse).ConfigureAwait(false);
    }
}





