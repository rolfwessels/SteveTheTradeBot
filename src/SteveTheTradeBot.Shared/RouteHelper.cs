namespace SteveTheTradeBot.Shared
{
    public class RouteHelper
    {
        public const string ApiPrefix = "api/";
        public const string WithId = "{id}";
        public const string WithDetail = "detail";
        public const string ProjectController = ApiPrefix + "project";
        public const string UserController = ApiPrefix + "user";
        public const string UserControllerRegister = "register";
        public const string UserControllerForgotPassword = "forgotpassword";
        public const string UserControllerWhoAmI = "whoami";
        public const string UserControllerRoles = "roles";
        public const string PingController = ApiPrefix + "ping";
        public const string PingControllerHealthCheck = "hc";
    }
}