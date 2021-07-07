using System;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using GraphQL;

namespace SteveTheTradeBot.Sdk.RestApi
{
    public class GraphQlResponseException<T> : Exception
    {
        public GraphQlResponseException(GraphQLResponse<T> graphQlResponse) : base(graphQlResponse.Errors
            .Select(x => x.Message).StringJoin())
        {
            GraphQlResponse = graphQlResponse;
        }

        public GraphQLResponse<T> GraphQlResponse { get; }
    }
}