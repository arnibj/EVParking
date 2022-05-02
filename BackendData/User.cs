using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;
using System.Security.Claims;

namespace BackendData
{
    public class User :DataBase
    {
        private readonly IMongoCollection<User> usersCollection;
        private ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public List<DateTime>? Logins { get; set; }
        public List<Role>? Roles { get; set; }

        public User()
        {
            usersCollection = db.GetCollection<User>("Users");
            Email = string.Empty;
            Name = string.Empty;
            Logins = new List<DateTime>();
        }

        /// <summary>
        /// Returns a list of all items from the collection
        /// </summary>
        /// <returns>Returns a list of all items from the collection</returns>
        public async Task<List<User>> GetItemsFromMongo()
        {
            try
            {
                List<User> list = new();

                if (cache.Get("Users") != null)
                {
                    list = (List<User>)cache.Get("Users");
                    return list;
                }
                User i = new();
                var result = await i.usersCollection.FindAsync(FilterDefinition<User>.Empty);
                list = result.ToList();
                cache.Add("Users", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<User>();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            List<User> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Id == id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            List<User> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(string email)
        {
            List<User> items = await GetItemsFromMongo();
            return items.SingleOrDefault(l => l.Email == email);
        }

        public async Task<bool> DoesUserExist(string email)
        {
            List<User> items = await GetItemsFromMongo();
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
                List<DateTime> dates = new()
                {
                    DateTime.Now
                };

                User appUser = new()
                {
                    Name = name,
                    Email = username,
                    Logins = dates,
                    Roles = await InitUserRoles()
                };
                await usersCollection.InsertOneAsync(appUser);
                cache.Remove("Users");
            }
            return true;
        }

        /// <summary>
        /// Helper to add user to the users role
        /// </summary>
        /// <returns>List of roles the user is added to</returns>
        private static async Task<List<Role>> InitUserRoles()
        {
            Role role = new();
            List<Role> allRoles = await role.Get();
            return new List<Role>() { allRoles.Single(r => r.Name == "User") };
        }
    }
}
