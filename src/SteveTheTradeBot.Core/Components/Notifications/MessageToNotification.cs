using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Notifications
{
    public class MessageToNotification
    {
        private readonly INotificationChannel _notification;

        public MessageToNotification(INotificationChannel notification)
        {
            _notification = notification;
        }

        public async Task OnTradeOrderMade(TradeOrderMadeMessage tradeOrder)
        {
            var total = Amount.From(tradeOrder.Order.Total, tradeOrder.Order.PaidCurrency);
            var buySell = Amount.From(tradeOrder.Order.OriginalQuantity, tradeOrder.Dump("").Order.FeeCurrency);
            var price = Amount.From(tradeOrder.Order.OrderPrice, tradeOrder.Order.PaidCurrency);
            if (tradeOrder.Order.OrderSide == Side.Buy)
            {
                await _notification.PostAsync(
                    $"{tradeOrder.StrategyInstance.Name} just *bought* {buySell} for *{total}* at {price}! :robot_face:");
            }
            else
            {
                total = Amount.From(tradeOrder.Order.Total, tradeOrder.Order.FeeCurrency);
                price = Amount.From(tradeOrder.Order.OrderPrice, tradeOrder.Order.FeeCurrency);
                buySell = Amount.From(tradeOrder.Order.OriginalQuantity, tradeOrder.Order.PaidCurrency);
                if (tradeOrder.StrategyTrade.IsProfit())
                {
                    await _notification.PostSuccessAsync(
                        $"{tradeOrder.StrategyInstance.Name} just *sold* *{buySell}* for {total} at {price}! We made {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)}({Percent(tradeOrder.StrategyTrade.Profit)}) :moneybag:");
                }
                else
                {
                    await _notification.PostFailedAsync(
                        $"{tradeOrder.StrategyInstance.Name} just *sold* *{buySell}* for {total} at {price}! We lost {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)}({Percent(tradeOrder.StrategyTrade.Profit)}) :money_with_wings:");
                }
            }
        }


        private string Percent(decimal strategyTradeProfit)
        {
            return Math.Round(strategyTradeProfit) + "%";
        }
    }
}