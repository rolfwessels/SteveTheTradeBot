using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Projects
{
    public class Project : BaseDalModelWithId
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Project: {Name}";
        }
    }
}