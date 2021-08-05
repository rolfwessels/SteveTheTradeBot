using System.Threading.Tasks;
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
            var cost = Amount.From(tradeOrder.Order.Total, tradeOrder.Order.PaidCurrency);
            var buySell = Amount.From(tradeOrder.Order.OriginalQuantity, tradeOrder.Order.FeeCurrency);
            var price = Amount.From(tradeOrder.Order.OrderPrice, tradeOrder.Order.PaidCurrency);
            if (tradeOrder.Order.OrderSide == Side.Buy)
            {
                await _notification.PostAsync(
                    $"{tradeOrder.StrategyInstance.Reference} just *bought* {buySell} for *{cost}* at {price}! :robot_face:");
            }
            else
            {
                price = Amount.From(tradeOrder.Order.OrderPrice, tradeOrder.Order.FeeCurrency);
                if (tradeOrder.StrategyTrade.IsProfit())
                {
                    await _notification.PostSuccessAsync(
                        $"{tradeOrder.StrategyInstance.Reference} just *sold* *{cost}* for {buySell} at {price}! We made {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)} :moneybag:");
                }
                else
                {
                    await _notification.PostFailedAsync(
                        $"{tradeOrder.StrategyInstance.Reference} just *sold* *{cost}* for {buySell} at {price}! We lost {Amount.From(tradeOrder.StrategyTrade.PriceDifference(), tradeOrder.Order.FeeCurrency)} :money_with_wings:");
                }
            }
        }

        
    }
}