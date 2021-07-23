using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public interface IStrategyRunner
    {
        Task Process(StrategyInstance strategyInstance, DateTime time);
    }
}