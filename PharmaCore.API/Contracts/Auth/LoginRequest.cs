namespace PharmaCore.API.Contracts.Auth;

/// <summary>
/// Request body for user login.
/// </summary>
/// <param name="UserName">Username for authentication.</param>
/// <param name="Password">Password for authentication.</param>
public sealed record LoginRequest(string UserName, string Password);
