using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using Serilog.Sinks.Loki.Labels;
using SteveTheTradeBot.Core.Framework.Settings;

namespace SteveTheTradeBot.Cmd
{
    public class LokiLogLabelProvider : ILogLabelProvider
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<LokiLabel> _lokiLabels;

        public LokiLogLabelProvider()
        {
            _lokiLabels = new List<LokiLabel>
            {
                new LokiLabel("app", "steve-the-trading-bot"),
                new LokiLabel("appenv", ConfigurationBuilderHelper.GetEnvironment().ToLower()),
                new LokiLabel("appversion", ConfigurationBuilderHelper.InformationalVersion()),
                new LokiLabel("machinename",  Environment.MachineName)
            };
        }

        #region Implementation of ILogLabelProvider

        public IList<LokiLabel> GetLabels()
        {
            return _lokiLabels;
        }

        #endregion

        public IList<string> PropertiesAsLabels { get; set; } = new List<string>
        {
            "level", // Since 3.0.0, you need to explicitly add level if you want it!
            "MyLabelPropertyName"
        };
        public IList<string> PropertiesToAppend { get; set; } = new List<string>
        {
            "MyAppendPropertyName"
        };
        
    }

}