namespace Garimpei.Exceptions;

/// <summary>Recurso não encontrado → traduzido para HTTP 404 pelo ErroMiddleware.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string mensagem) : base(mensagem) { }
}

/// <summary>Conflito de regra de negócio/estado → traduzido para HTTP 409 pelo ErroMiddleware.</summary>
public class ConflictException : Exception
{
    public ConflictException(string mensagem) : base(mensagem) { }
}

/// <summary>Erro de validação de negócio simples → traduzido para HTTP 400 pelo ErroMiddleware.</summary>
public class ValidationException : Exception
{
    public ValidationException(string mensagem) : base(mensagem) { }
}
