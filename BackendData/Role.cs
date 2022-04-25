using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;

namespace BackendData
{
    public class Role : DataBase
    {
        private readonly IMongoCollection<Role> rowCollection;
        private readonly ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Role()
        {
            rowCollection = db.GetCollection<Role>("Roles");
            Name = string.Empty;
        }

        /// <summary>
        /// Retrieves list of roles
        /// </summary>
        /// <returns>List of all roles, from cache or datastore</returns>
        public async Task<List<Role>> Get()
        {
            try
            {
                List<Role> list = new();
                if (cache.Get("Roles") != null)
                {
                    list = (List<Role>)cache.Get("Roles");
                    return list;
                }
                Role i = new();
                var result = await i.rowCollection.FindAsync(FilterDefinition<Role>.Empty);
                list = result.ToList()
                    .OrderBy(o => o.Name)
                    .ToList();

                if (list.Count == 0)
                {
                    list = await CreateRoles();
                }

                cache.Add("Roles", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<Role>();
        }

        /// <summary>
        /// Adds row to the underlying datastore
        /// </summary>
        /// <param name="role">Role object</param>
        /// <returns>True if stored successfully</returns>
        public async Task<bool> Add(Role role)
        {
            await rowCollection.InsertOneAsync(role);
            cache.Remove("Roles");
            return true;
        }

        /// <summary>
        /// Add basic roles needed
        /// </summary>
        public async Task<List<Role>> CreateRoles()
        {
            Role user = new()
            {
                Name = "User",
                Id = Guid.NewGuid()
            };
            await Add(user);

            Role admin = new()
            {
                Name = "Administrator",
                Id = Guid.NewGuid()
            };
            await Add(admin);

            List<Role> list = new();
            list.Add(user);
            list.Add(admin);

            return list;
        }
    }


}
