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
            var cost = Amount.From(tradeOrder.Order.OutQuantity, tradeOrder.Order.OutCurrency);
            var buySell = Amount.From(tradeOrder.Order.OriginalQuantity, tradeOrder.Order.FeeCurrency);
            var price = Amount.From(tradeOrder.Order.OrderPrice, tradeOrder.Order.OutCurrency);
            if (tradeOrder.Order.OrderSide == Side.Buy)
            {
                await _notification.PostAsync(
                    $"{tradeOrder.StrategyInstance.StrategyName} just *bought* {buySell} for *{cost}* at {price}! :money_with_wings:");
            }
            else
            {
                if (tradeOrder.StrategyTrade.IsProfit())
                {
                    await _notification.PostAsync(
                        $"{tradeOrder.StrategyInstance.StrategyName} just *sold* *{cost}* for {buySell} at {price}! We made {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)} :money_with_wings:");
                }
                else
                {
                    await _notification.PostAsync(
                        $"{tradeOrder.StrategyInstance.StrategyName} just *sold* *{cost}* for {buySell} at {price}! We lost {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)} :money_with_wings:");
                }
            }
        }

        
    }
}