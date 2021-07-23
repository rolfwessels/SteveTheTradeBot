using System;
using System.Collections.Generic;
using System.Linq;
using Bumbershoot.Utilities.Helpers;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class StrategyPicker
    {
        Dictionary<string, Func<IStrategy>> _holder;

        public StrategyPicker()
        {
            _holder = new Dictionary<string, Func<IStrategy>>();
        }

        public List<string> List => _holder.Keys.ToList();

        public StrategyPicker Add(string name, Func<IStrategy> factory)
        {
            _holder.Add(name, factory);
            return this;
        }

        public IStrategy Get(string name)
        {
            if (_holder.ContainsKey(name))
            {
                var func = _holder[name];
                return func();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(name),$"Could not find strategy {name}, options are {_holder.Keys.StringJoin()}");
            }
        }
    }
}