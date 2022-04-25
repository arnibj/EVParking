using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;
using System.Security.Claims;

namespace EVParking.Models
{
    public class PushClient : DataBase
    {
        private readonly IMongoCollection<PushClient> rowCollection;
        private ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid ClientId { get; set; }
        public string? P256dh { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string? Endpoint { get; set; }
        public string? Auth { get; set; }
        public DateTime DateAdded { get; set; }
        public string? UserName { get; set; }

        public PushClient()
        {
            rowCollection = db.GetCollection<PushClient>("PushClients");
        }

        public async Task<bool> AddClient(PushClient p)
        {
            if (p == null)
                return false;

            if (p.UserName == null)
                return false;

                PushClient client = new()
                {
                    ClientId = p.ClientId,
                    P256dh = p.P256dh,
                    ExpirationTime = p.ExpirationTime,
                    Endpoint = p.Endpoint, 
                    Auth = p.Auth,
                    DateAdded = p.DateAdded,
                    UserName = p.UserName
                };
                await rowCollection.InsertOneAsync(client);
                cache.Remove("PushClients");
            
            return true;
        }

        public async Task<bool> RemoveClient(PushClient p)
        {
            if (p == null)
                return false;

            FilterDefinition<PushClient> idFilterDefinition = Builders<PushClient>.Filter.Eq(client => client.ClientId, p.ClientId);
            await rowCollection.DeleteOneAsync(idFilterDefinition);
            cache.Remove("PushClients");
            return true;
        }

        /// <summary>
        /// Returns a list of all items from the collection
        /// </summary>
        /// <returns>Returns a list of all items from the collection</returns>
        public async Task<List<PushClient>> GetItems()
        {
            try
            {
                List<PushClient> list = new();

                if (cache.Get("PushClients") != null)
                {
                    list = (List<PushClient>)cache.Get("PushClients");
                    return list;
                }
                PushClient i = new();
                var result = await i.rowCollection.FindAsync(FilterDefinition<PushClient>.Empty);
                list = result.ToList();
                cache.Add("PushClients", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<PushClient>();
        }

        public async Task<PushClient?> GetClientByIdAsync(Guid clientid)
        {
            List<PushClient> items = await GetItems();
            return items.SingleOrDefault(l => l.ClientId == clientid);
        }
        public static string ReturnUserClaimTypeValue(ClaimsIdentity identity, string type)
        {
            try
            {
                if (identity != null && identity.Claims != null)
                {
                    Claim v = identity.Claims.Where(c => c.Type == type).SingleOrDefault();
                    if (v != null)
                        return v.Value;
                }
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }


    }
}
