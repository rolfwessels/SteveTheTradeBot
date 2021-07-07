namespace SteveTheTradeBot.Dal.Models.Auth
{
    public enum Activity
    {
        Subscribe = 001,

        ReadUsers = 100,
        UpdateUsers = 101,
        DeleteUser = 103,

        ReadProject = 200,
        UpdateProject = 201,
        DeleteProject = 203
    }
}