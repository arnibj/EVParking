using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;

namespace EVParking.Models
{
    public class MongoUser :DataBase
    {
        private readonly IMongoCollection<ApplicationUser> usersCollection;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string UserName { get; set; }

        public MongoUser()
        {
            usersCollection = db.GetCollection<ApplicationUser>("Users");
            UserName = string.Empty;
        }

        /// <summary>
        /// Returns a list of all items from the collection
        /// </summary>
        /// <returns>Returns a list of all items from the collection</returns>
        public async Task<List<ApplicationUser>> GetItemsFromMongo()
        {
            try
            {
                List<ApplicationUser> list = new();
                ObjectCache cache = MemoryCache.Default;
                if (cache.Get("MongoUsers") != null)
                {
                    list = (List<ApplicationUser>)cache.Get("MongoUsers");
                    return list;
                }
                MongoUser i = new();
                var result = await i.usersCollection.FindAsync(FilterDefinition<ApplicationUser>.Empty);
                list = result.ToList();
                cache.Add("MongoUsers", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
                return null;
            }
        }

        public async Task<ApplicationUser> GetUserByIdAsync(Guid id)
        {
            List<ApplicationUser> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Id == id);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string username)
        {
            List<ApplicationUser> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.UserName == username);
        }

        public async Task<bool> DoesUserExist(string username)
        {
            List<ApplicationUser> items = await GetItemsFromMongo();
            return items.Any(l => l.UserName == username);
        }
    }
}
