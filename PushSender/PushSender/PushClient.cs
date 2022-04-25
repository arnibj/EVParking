using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushSender
{
    public class PushClient
    {
        public int Id { get; set; }
        public Guid ClientId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Endpoint { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string p256dh { get; set; }
        public string auth { get; set; }
        public async Task<List<PushClient>> GetClients()
        {
            try
            {
                List<PushClient> list = new List<PushClient>();

                
                PushClient i = new PushClient();
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
    }
}
