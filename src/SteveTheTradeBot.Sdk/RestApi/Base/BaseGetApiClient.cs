using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Shared;
using SteveTheTradeBot.Shared.Interfaces.Base;
using SteveTheTradeBot.Shared.Models.Shared;
using RestSharp;

namespace SteveTheTradeBot.Sdk.RestApi.Base
{
    public class BaseGetApiClient<TModel, TReferenceModel> : BaseApiClient,
        IBaseStandardLookups<TModel, TReferenceModel>
        where TModel : IBaseModel, new()
    {
        public BaseGetApiClient(SteveTheTradeBotClient dockerClient, string baseUrl)
            : base(dockerClient, baseUrl)
        {
        }

        #region IBaseStandardLookups<TModel,TReferenceModel> Members

        public async Task<PagedResult<TReferenceModel>> GetPaged(string oDataQuery)
        {
            var restRequest = new RestRequest(DefaultUrl($"?{EnsureHasInlinecount(oDataQuery)}"));
            var executeAsyncWithLogging =
                await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<PagedResult<TReferenceModel>>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<PagedResult<TModel>> GetDetailPaged(string oDataQuery)
        {
            var restRequest =
                new RestRequest(DefaultUrl($"{RouteHelper.WithDetail}?{EnsureHasInlinecount(oDataQuery)}"));
            var executeAsyncWithLogging =
                await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<PagedResult<TModel>>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<IEnumerable<TReferenceModel>> Get(string oDataQuery)
        {
            var restRequest = new RestRequest(DefaultUrl($"?{oDataQuery}"));
            var executeAsyncWithLogging =
                await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<List<TReferenceModel>>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        public async Task<IEnumerable<TModel>> GetDetail(string oDataQuery)
        {
            var restRequest = new RestRequest(DefaultUrl($"{RouteHelper.WithDetail}?{oDataQuery}"));
            var executeAsyncWithLogging =
                await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<List<TModel>>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }

        #endregion

        #region Private Methods

        private static string EnsureHasInlinecount(string oDataQuery)
        {
            if (oDataQuery == null || !oDataQuery.Contains("$inlinecount"))
                oDataQuery = $"{oDataQuery}&$inlinecount=allpages";
            return oDataQuery;
        }

        #endregion

        #region Implementation of IBaseStandardLookups<UserModel,UserReferenceModel>

        public Task<IEnumerable<TReferenceModel>> Get()
        {
            return Get("");
        }

        public Task<IEnumerable<TModel>> GetDetail()
        {
            return GetDetail("");
        }

        #endregion
    }
}