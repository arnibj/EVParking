using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendData
{
    public class PushNotification
    {
        public int Id { get; set; }
        public Guid ClientId { get; set; }
        public DateTime SentDate { get; set; }
        public bool Sent { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Badge { get; set; }
        public string? Icon { get; set; }
        public bool Received { get; set; }
        public PushClient? PushClient { get; set; }

        public PushClient? GetPushClient()
        {
            return PushClient;
        }

        public async Task<List<PushNotification>> GetUnsentPushNotifications(PushClient? pushClient)
        {
            List<PushNotification> test = new();
            pushClient = new();
            List<PushClient>? clients = await pushClient.GetItems();

            if(clients.Count > 0)
            {
                PushNotification m = new()
                {
                    Id = 1,
                    Badge = string.Empty,
                    Title = "Test",
                    Body = "Test Body",
                    Icon = string.Empty,
                    ClientId = clients[0].ClientId,
                    PushClient = clients[0]
                };
                test.Add(m);
            }
            return test;
        }
    }
}
