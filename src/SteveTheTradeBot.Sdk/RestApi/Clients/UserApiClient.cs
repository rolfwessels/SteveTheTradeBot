using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.RestApi.Base;
using SteveTheTradeBot.Shared;
using SteveTheTradeBot.Shared.Models;
using SteveTheTradeBot.Shared.Models.Users;
using Bumbershoot.Utilities.Helpers;
using GraphQL;

namespace SteveTheTradeBot.Sdk.RestApi.Clients
{
    public class UserApiClient : BaseApiClient
    {
        public UserApiClient(SteveTheTradeBotClient dockerClient)
            : base(dockerClient, RouteHelper.UserController)
        {
        }

        public async Task<List<UserModel>> List()
        {
            var request = new GraphQLRequest
            {
                Query = GraphQlFragments.User + @"{
                    users {
                        paged() {
                            items {...userData}
                        }
                    }
                }"
            };
            var response = await SteveTheTradeBotClient.Post<Response>(request);
            return response.Data.Users.Paged.Items;
        }

        public async Task<UserModel> ById(string id)
        {
            var request = new GraphQLRequest
            {
                Query = GraphQlFragments.User + @"query ($id: String!) {
                  users {
                    byId(id: $id) {
                      ...userData
                    }
                  }
                }",
                Variables = new {id}
            };
            var response = await SteveTheTradeBotClient.Post<Response>(request);
            return response.Data.Users.ById;
        }

        public async Task<UserModel> Me()
        {
            var request = new GraphQLRequest
            {
                Query = GraphQlFragments.User + @"{
                    users {
                        me {
                            ...userData
                        }
                    }
                }"
            };
            var response = await SteveTheTradeBotClient.Post<Response>(request);
            return response.Data.Users.Me;
        }

        public async Task<CommandResultModel> Create(UserCreateUpdateModel user)
        {
            var response = await SteveTheTradeBotClient.Post<Response>(new GraphQLRequest
            {
                Query = GraphQlFragments.CommandResult + @"
                mutation ($name: String!, $email: String!, $roles: [String!]!, $password: String) {
                  users {
                    create(user: {name: $name, email: $email, roles: $roles, password: $password}) {
                      ...commandResultData
                    }
                  }
                }",
                Variables = new {user.Name, user.Email, user.Roles, user.Password}
            });
            return response.Data.Users.Create;
        }

        public async Task<CommandResultModel> Register(RegisterModel user)
        {
            var response = await SteveTheTradeBotClient.Post<Response>(new GraphQLRequest
            {
                Query = GraphQlFragments.CommandResult + @"
                mutation ($name: String!, $email: String!, $password: String!) {
                  users {
                    register(user: {name: $name, email: $email, password: $password}) {
                      ...commandResultData
                    }
                  }
                }",
                Variables = new {user.Name, user.Email, user.Password}
            });
            return response.Data.Users.Register;
        }

        public async Task<CommandResultModel> Update(string id, UserCreateUpdateModel user)
        {
            var response = await SteveTheTradeBotClient.Post<Response>(new GraphQLRequest
            {
                Query = GraphQlFragments.CommandResult + @"
                mutation ($id: String!, $name: String!, $email: String!, $roles: [String!]!, $password: String) {
                  users {
                    update(id: $id, user: {name: $name, email: $email, roles: $roles, password: $password}) {
                      ...commandResultData
                    }
                  }
                }",
                Variables = new {id, user.Name, user.Email, user.Roles, user.Password}
            });

            return response.Data.Users.Update;
        }

        public async Task<CommandResultModel> Remove(string id)
        {
            var response = await SteveTheTradeBotClient.Post<Response>(new GraphQLRequest
            {
                Query = GraphQlFragments.CommandResult + @"
                mutation ($id: String!) {
                  users {
                    remove(id: $id) {
                        ...commandResultData
                    }
                  }
                }",
                Variables = new {id}
            });

            return response.Data.Users.Remove;
        }


        public async Task<List<RoleModel>> Roles()
        {
            var response = await SteveTheTradeBotClient.Post<Response>(new GraphQLRequest
            {
                Query = @"{
                  users {
                    roles {
                      name
                      activities
                    }
                  }
                }"
            });
            return response.Data.Users.Roles;
        }


        public async Task<PagedListModel<UserModel>> Paged(int? first = null)
        {
            var request = new GraphQLRequest
            {
                Query = GraphQlFragments.User + @"query ($first: Int){
                    users {
                        paged(first:$first, includeCount: true) {
                            count,
                            items {...userData}
                        }
                    }
                }",
                Variables = new {first}
            };
            var response = await SteveTheTradeBotClient.Post<Response>(request);
            return response.Data.Users.Paged;
        }

        private class Response
        {
            public ResponseData Users { get; set; }

            public class ResponseData
            {
                public CommandResultModel Register { get; set; }
                public UserModel Me { get; set; }
                public List<RoleModel> Roles { get; set; }
                public PagedListModel<UserModel> Paged { get; set; }
                public UserModel ById { get; set; }
                public CommandResultModel Create { get; set; }
                public CommandResultModel Update { get; set; }
                public CommandResultModel Remove { get; set; }
            }
        }
    }
}