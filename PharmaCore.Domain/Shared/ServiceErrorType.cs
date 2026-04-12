namespace PharmaCore.Domain.Shared;

public enum ServiceErrorType
{
    None,
    NotFound,
    Duplicate,
    Validation,
    ServerError,
    Unauthorized
}
