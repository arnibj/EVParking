using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;
using System.Security.Claims;

namespace BackendData
{
    public class MongoUser :DataBase
    {
        private readonly IMongoCollection<MongoUser> usersCollection;
        private ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public List<DateTime>? Logins { get; set; }

        //public List<ApplicationRole>? Roles { get; set; }

        public MongoUser()
        {
            usersCollection = db.GetCollection<MongoUser>("Users");
            Email = string.Empty;
            Name = string.Empty;
            Logins = new List<DateTime>();
            //Roles = new List<ApplicationRole>();

            //ApplicationRole applicationRole = new();
            //applicationRole.Name = "User";

            //Roles.Add(applicationRole);
        }

        /// <summary>
        /// Returns a list of all items from the collection
        /// </summary>
        /// <returns>Returns a list of all items from the collection</returns>
        public async Task<List<MongoUser>> GetItemsFromMongo()
        {
            try
            {
                List<MongoUser> list = new();

                if (cache.Get("MongoUsers") != null)
                {
                    list = (List<MongoUser>)cache.Get("MongoUsers");
                    return list;
                }
                MongoUser i = new();
                var result = await i.usersCollection.FindAsync(FilterDefinition<MongoUser>.Empty);
                list = result.ToList();
                cache.Add("MongoUsers", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<MongoUser>();
        }

        public async Task<MongoUser?> GetUserByIdAsync(Guid id)
        {
            List<MongoUser> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Id == id);
        }

        public async Task<MongoUser?> GetUserByIdAsync(string email)
        {
            List<MongoUser> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Email == email);
        }

        public async Task<bool> DoesUserExist(string email)
        {
            List<MongoUser> items = await GetItemsFromMongo();
            return items.Any(l => l.Email == email);
        }

        public async Task<bool> AddUser(IEnumerable<Claim> claims)
        {
            if (!claims.Any())
                return false;

            string? username = claims.SingleOrDefault(c => c.Type.Equals("preferred_username"))?.Value;
            string? name = claims.SingleOrDefault(c => c.Type.Equals("name"))?.Value;

            if (username == null)
                return false;

            bool userExists = await DoesUserExist(username);
            if (!userExists)
            {
                List<DateTime> dates = new List<DateTime>();
                dates.Add(DateTime.Now);
                MongoUser appUser = new()
                {
                    Name = name,
                    Email = username,
                    Logins = dates
                };
                await usersCollection.InsertOneAsync(appUser);
                cache.Remove("MongoUsers");
            }
            return true;
        }
    }
}
