using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.ErrorHandling;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate? _next;
    private readonly GatewayErrorHandler _errorHandler;
    private readonly ILogger<ErrorHandlingMiddleware>? _logger;
    
    private static readonly Action<ILogger, Exception?> LogUnhandledException =
        LoggerMessage.Define(LogLevel.Error, new EventId(3001, nameof(LogUnhandledException)), 
            "Unhandled exception occurred in request pipeline");
            
    private static readonly Action<ILogger, Exception?> LogResponseAlreadyStarted =
        LoggerMessage.Define(LogLevel.Warning, new EventId(3002, nameof(LogResponseAlreadyStarted)), 
            "Cannot handle error - response has already started");
    
    public ErrorHandlingMiddleware(RequestDelegate next, GatewayErrorHandler errorHandler)
    {
        _next = next;
        _errorHandler = errorHandler;
    }
    
    // GREEN PHASE: Constructor for testing
    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, GatewayErrorHandler errorHandler)
    {
        _logger = logger;
        _errorHandler = errorHandler;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // GREEN PHASE: Minimal implementation for medical-grade error handling middleware
            if (_next != null)
            {
                await _next(context).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            if (_logger != null)
            {
                LogUnhandledException(_logger, ex);
            }
            
            // Avoid handling errors if response has already started
            if (context.Response.HasStarted)
            {
                if (_logger != null)
                {
                    LogResponseAlreadyStarted(_logger, null);
                }
                throw;
            }
            
            // Clear any existing response
            context.Response.Clear();
            
            // Handle the error
            await _errorHandler.HandleErrorAsync(context, ex).ConfigureAwait(false);
        }
    }
}
