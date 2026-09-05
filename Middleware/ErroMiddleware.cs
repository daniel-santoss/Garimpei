using Garimpei.Exceptions;

namespace Garimpei.Middleware;

/// <summary>
/// Captura exceções e as traduz para o envelope de erro padrão:
/// { "sucesso": false, "mensagem": "..." } com o status HTTP adequado.
/// (ver ENGENHARIA-GARIMPEI.md, Seção 19)
/// </summary>
public class ErroMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErroMiddleware> _log;

    public ErroMiddleware(RequestDelegate next, ILogger<ErroMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (NotFoundException ex)
        {
            await EscreverAsync(ctx, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await EscreverAsync(ctx, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ValidationException ex)
        {
            await EscreverAsync(ctx, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Erro não tratado em {Path}", ctx.Request.Path);
            await EscreverAsync(ctx, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado. Tente novamente.");
        }
    }

    private static Task EscreverAsync(HttpContext ctx, int status, string mensagem)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        return ctx.Response.WriteAsJsonAsync(new { sucesso = false, mensagem });
    }
}
