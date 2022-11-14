using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;
using System.Security.Claims;

namespace BackendData
{
    public class PushClient : DataBase
    {
        /// <summary>
        /// Class to handle Push clients, which represents devices that can be used to send push notifications by the system
        /// </summary>
        private readonly IMongoCollection<PushClient> clientCollection;
        private readonly ObjectCache cache = MemoryCache.Default;

        #region Properties
        [BsonId]
        [BsonElement("_id")]
        public Guid ClientId { get; set; }
        public string? P256dh { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string? Endpoint { get; set; }
        public string? Auth { get; set; }
        public DateTime DateAdded { get; set; }
        public Guid UserId { get; set; }
        #endregion

        public PushClient()
        {
            clientCollection = db.GetCollection<PushClient>("PushClients");
        }

        /// <summary>
        /// Add new client that can be used to receive push notifications
        /// </summary>
        /// <param name="p">PushClient object</param>
        /// <returns>True if stored successfully</returns>
        public async Task<bool> AddClient(PushClient p)
        {
            if (p == null)
                return false;

            PushClient client = new()
            {
                ClientId = p.ClientId,
                P256dh = p.P256dh,
                ExpirationTime = p.ExpirationTime,
                Endpoint = p.Endpoint,
                Auth = p.Auth,
                DateAdded = p.DateAdded,
                UserId = p.UserId
            };
            await clientCollection.InsertOneAsync(client);
            cache.Remove("PushClients");
            cache.Remove("PushClients_" + p.UserId);
            return true;
        }

        /// <summary>
        /// Removes inputted client & refreshes cache
        /// </summary>
        /// <param name="p">Client to remove</param>
        /// <returns>true if successful</returns>
        public async Task<bool> RemoveClient(PushClient p)
        {
            if (p == null)
                return false;

            try
            {
                FilterDefinition<PushClient> idFilterDefinition = Builders<PushClient>.Filter.Eq(client => client.ClientId, p.ClientId);
                await clientCollection.DeleteOneAsync(idFilterDefinition);
                cache.Remove("PushClients");
                cache.Remove("PushClients_" + p.UserId);
                return true;
            }
            catch
            {
            }
            return false;
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
                var result = await i.clientCollection.FindAsync(FilterDefinition<PushClient>.Empty);
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

        public async Task<List<PushClient>> GetItemsByUserId(Guid userId)
        {
            try
            {
                List<PushClient> list = new();

                if (cache.Get("PushClients_" + userId) != null)
                {
                    list = (List<PushClient>)cache.Get("PushClients_" + userId);
                    return list;
                }
                PushClient i = new();
                FilterDefinition<PushClient> filter = Builders<PushClient>.Filter.Eq(f => f.UserId, userId);
                var result = await i.clientCollection.FindAsync(filter);
                list = result.ToList();
                cache.Add("PushClients_" + userId, list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<PushClient>();
        }

        /// <summary>
        /// Get client by clientId
        /// </summary>
        /// <param name="clientid">Guid client id</param>
        /// <returns>PushClient object if found, null otherwise</returns>
        public async Task<PushClient?> GetClientByIdAsync(Guid clientid)
        {
            List<PushClient> items = await GetItems();
            return items.SingleOrDefault(l => l.ClientId == clientid);
        }
    }
}
