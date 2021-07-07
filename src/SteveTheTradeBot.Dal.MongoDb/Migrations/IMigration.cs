using System.Threading.Tasks;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb.Migrations
{
    public interface IMigration
    {
        Task Update(IMongoDatabase db);
    }
}