using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace BackendData
{
    public class Notification :DataBase
    {
        private readonly IMongoCollection<Notification> rowCollection;
        private readonly ObjectCache cache = MemoryCache.Default;

        #region Properties
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime SentDate { get; set; }
        public bool Sent { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Badge { get; set; }
        public string? Icon { get; set; }
        public bool Received { get; set; }
        public List<PushClient>? PushClients { get; set; }
        #endregion

        public Notification()
        {
            rowCollection = db.GetCollection<Notification>("Notifications");
        }

        public async Task<List<Notification>> GetUnsentNotifications()
        {
            List<Notification> test = new();
            PushClient pushClient = new();
            List<PushClient>? clients = await pushClient.GetItems();

            if(clients.Count > 0)
            {
                Notification m = new()
                {
                    Id = 1,
                    Badge = string.Empty,
                    Title = "Test",
                    Body = "Test Body",
                    Icon = "../images/icon-192x192.png",
                    UserId = clients[0].UserId,
                    PushClients = new List<PushClient>
                    {
                        clients[0]
                    }
                };
                test.Add(m);
            }
            return test;
        }

        /// <summary>
        /// Get notifications that belong to specific user
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <returns>List of notifications</returns>
        public async Task<List<Notification>> GetUserNotifications(Guid userId, DateTime? createdAfterDateTime)
        {
            List<Notification> notifications = new();
            FilterDefinition<Notification> filter = Builders<Notification>.Filter.Eq(message => message.UserId, userId);
            if (createdAfterDateTime.HasValue)
            {
                FilterDefinition<Notification> dateFilterDefinition = Builders<Notification>.Filter.Gt(n => n.SentDate, createdAfterDateTime);
                filter = Builders<Notification>.Filter.And(dateFilterDefinition);
            }
            var result = await rowCollection.FindAsync(filter);
            notifications = result.ToList();

            PushClient p = new();
            List<PushClient> userClients = await p.GetItemsByUserId(userId);

            foreach(Notification notification in notifications)
            {
                notification.PushClients = userClients;
            }

            return notifications;
        }
    }
}
