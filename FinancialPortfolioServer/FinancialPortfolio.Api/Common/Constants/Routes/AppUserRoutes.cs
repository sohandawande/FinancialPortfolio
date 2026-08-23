namespace FinancialPortfolio.Api.Common.Constants.Routes
{
    public static class AppUserRoutes
    {
        public const string Pending = "pending";
        public const string ManageUsers = "manage-users";
        public const string Approve = "approve/{identityUserId:guid}";
        public const string AssignRoles = "roles/{identityUserId:guid}";
        public const string Activate = "activate/{identityUserId:guid}";
        public const string Deactivate = "deactivate/{identityUserId:guid}";
        public const string GetById = "get-user-by-id/{identityUserId:guid}";
    }
}
