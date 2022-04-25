using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushSender
{
    public class PushMessage
    {
        public int Id { get; set; }
        public Guid ClientId { get; set; }
        public DateTime SentDate { get; set; }
        public bool Sent { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Badge { get; set; }
        public string Icon { get; set; }
        public bool Received { get; set; }
        public PushClient PushClient { get; set; }
        public List<PushMessage> GetUnsentPushMessages()
        {
            List<PushMessage> test = new List<PushMessage>();
            PushMessage m = new PushMessage();
            m.Id = 1;
            m.Badge = String.Empty;
            m.Title = "Test";
            m.Body = "Test Body";
            m.Icon = String.Empty;
            test.Add(m);

            return test;
        }
    }
}
