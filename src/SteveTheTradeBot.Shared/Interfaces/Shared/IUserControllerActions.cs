using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Shared.Interfaces.Base;
using SteveTheTradeBot.Shared.Models.Users;

namespace SteveTheTradeBot.Shared.Interfaces.Shared
{
    public interface IUserControllerActions : ICrudController<UserModel, UserCreateUpdateModel>
    {
        Task<UserModel> Register(RegisterModel user);

//        Task<bool> ForgotPassword(string email);
        Task<UserModel> WhoAmI();
        Task<List<RoleModel>> Roles();
    }
}