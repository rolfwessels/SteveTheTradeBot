using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;

namespace SteveTheTradeBot.Api.Lambda
{
    /// <summary>
    ///     This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the
    ///     actual Lambda function entry point. The Lambda handler field should be set to
    ///     SteveTheTradeBot.Api.Lambda::SteveTheTradeBot.Api.Lambda.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint : APIGatewayProxyFunction
    {
        /// <summary>
        ///     The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        ///     needs to be configured in this method using the UseStartup() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }
    }
}