namespace PharmaCore.API.Controllers;

internal static class ApiRoles
{
    public const string Admin = "ADMIN";
    public const string Cashier = "CASHIER";
    public const string Staff = Admin + "," + Cashier;
}
