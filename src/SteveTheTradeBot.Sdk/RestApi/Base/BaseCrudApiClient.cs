using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Shared;
using SteveTheTradeBot.Shared.Interfaces.Base;
using SteveTheTradeBot.Shared.Models.Shared;
using RestSharp;

namespace SteveTheTradeBot.Sdk.RestApi.Base
{
    public class BaseCrudApiClient<TModel, TDetailModel, TReferenceModel> : BaseGetApiClient<TModel, TReferenceModel>,
        ICrudController<TModel, TDetailModel>
        where TModel : IBaseModel, new()
    {
        protected BaseCrudApiClient(SteveTheTradeBotClient dockerClient, string baseUrl)
            : base(dockerClient, baseUrl)
        {
        }

        #region ICrudController<TModel,TDetailModel> Members

        public async Task<TModel> GetById(string id)
        {
            var restRequest = new RestRequest(DefaultUrl(RouteHelper.WithId.SetParam("id", id)));
            var executeAsyncWithLogging = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<TModel>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<TModel> Insert(TDetailModel model)
        {
            var restRequest = new RestRequest(DefaultUrl(), Method.POST);
            restRequest.AddJsonBody(model);
            var executeAsyncWithLogging = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<TModel>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<TModel> Update(string id, TDetailModel model)
        {
            var restRequest = new RestRequest(DefaultUrl(RouteHelper.WithId.SetParam("id", id)), Method.PUT);
            restRequest.AddJsonBody(model);
            var executeAsyncWithLogging = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<TModel>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<bool> Delete(string id)
        {
            var restRequest = new RestRequest(DefaultUrl(RouteHelper.WithId.SetParam("id", id)), Method.DELETE);
            var executeAsyncWithLogging = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<bool>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        #endregion
    }
}