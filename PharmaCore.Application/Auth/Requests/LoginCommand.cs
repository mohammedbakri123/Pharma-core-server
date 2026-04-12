namespace PharmaCore.Application.Auth.Requests;

public sealed record LoginCommand(string UserName, string Password);
