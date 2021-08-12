using System;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class DynamicPlotter :  BaseDalModel
    {   
        public string Feed { get; set; }
        public string Label { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Feed)}: {Feed}, {nameof(Label)}: {Label}, {nameof(Date)}: {Date}, {nameof(Value)}: {Value}";
        }
    }
}