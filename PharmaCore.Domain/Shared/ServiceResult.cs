namespace PharmaCore.Domain.Shared;

public class ServiceResult<T>
{
    public bool Success => Error.Type == ServiceErrorType.None;
    public ServiceError Error { get; set; } = new();
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Data = data, Error = new ServiceError() };

    public static ServiceResult<T> Fail(ServiceErrorType type, string message) =>
        new() { Error = new ServiceError { Type = type, Message = message } };
}
