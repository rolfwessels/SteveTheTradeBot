using System;
using System.ComponentModel.Design;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class SimpleOrderStatusResponse
    {
        public string OrderId { get; set; }
        public bool Success { get; set; }
        public bool Processing { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaidCurrency { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string ReceivedCurrency { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeCurrency { get; set; }
        public DateTime OrderExecutedAt { get; set; }

        public decimal OriginalPrice(Side sell)
        {
            if (sell == Side.Sell)
            {
                return Math.Floor(PaidAmount / (ReceivedAmount + FeeAmount));
            }

            return  (ReceivedAmount + FeeAmount) / PaidAmount;
        }
    }
}