using BackendData;
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
        private readonly IMongoCollection<PushClient> rowCollection;
        private ObjectCache cache = MemoryCache.Default;

        #region Properties
        [BsonId]
        [BsonElement("_id")]
        public Guid ClientId { get; set; }
        public string? P256dh { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string? Endpoint { get; set; }
        public string? Auth { get; set; }
        public DateTime DateAdded { get; set; }
        public string? UserName { get; set; }
        #endregion

        public PushClient()
        {
            rowCollection = db.GetCollection<PushClient>("PushClients");
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
                await rowCollection.DeleteOneAsync(idFilterDefinition);
                cache.Remove("PushClients");
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

        /// <summary>
        /// Helper function to aquire value from the user's claims property list
        /// </summary>
        /// <param name="identity">The users claims identity object</param>
        /// <param name="type">Name of property requested</param>
        /// <returns>Value of requested property if it exist, blank string otherwise</returns>
        public static string ReturnUserClaimTypeValue(ClaimsIdentity identity, string type)
        {
            try
            {
                if (identity != null && identity.Claims != null && !string.IsNullOrEmpty(type))
                {
                    var v = identity.Claims.Where(c => c.Type == type).SingleOrDefault();
                    if (v != null)
                        return v.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return String.Empty;
        }


    }
}
