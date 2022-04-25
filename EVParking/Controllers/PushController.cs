using BackendData;
using EVParking.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVParking.Controllers
{
    [ApiController]
    public class PushController : ControllerBase
    {
        public class Keys
        {
            public string? P256dh { get; set; }
            public string? Auth { get; set; }
        }
        public class PushNotificationSubscription
        {
            public string? Endpoint { get; set; }
            public DateTime? ExpirationTime { get; set; }
            public Keys? Keys { get; set; }
        }

        [HttpPost]
        [Route("api/push/subscription")]
        public async Task<string> pushsubscription(PushNotificationSubscription s)
        {
            Helper h = new();

            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("is-IS");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("is-IS");
                if (User != null && User.Identity != null)
                {
                    var user = PushClient.ReturnUserClaimTypeValue((ClaimsIdentity)User.Identity, "preferred_username");
                    if (s != null && !string.IsNullOrEmpty(user))
                    {
                        if (!s.ExpirationTime.HasValue)
                        {
                            s.ExpirationTime = DateTime.Now.AddYears(10);
                        }
                        Guid clientID = Guid.NewGuid();
                        PushClient p = new()
                        {
                            P256dh = s.Keys?.P256dh,
                            ExpirationTime = s.ExpirationTime,
                            Endpoint = s.Endpoint,
                            Auth = s.Keys?.Auth,
                            UserName = user,
                            DateAdded = DateTime.Now,
                            ClientId = clientID
                        };
                        PushClient pushClient = new();
                        await pushClient.AddClient(p);
                        return clientID.ToString();
                    }
                }
                else
                {
                    h.LogTrace("User identity could not be verified, client could not be stored");
                }
            }
            catch (Exception ex)
            {
                h.LogException(ex);
            }
            return String.Empty;
        }

        [HttpPost]
        [Route("api/push/unsubscribe")]
        public async Task<string> Unsubscribe(PushNotificationSubscription s)
        {
            PushClient p = new PushClient();
            var pushClient = await p.GetItems();
            var c = pushClient.FirstOrDefault(m => m.Auth == s.Keys?.Auth && m.P256dh == s.Keys?.P256dh);
            if (c != null)
            {
                bool success = await p.RemoveClient(c);
                if (success)
                    return "1";
                else
                    return "0";
            }
            
            return string.Empty;
        }
    }
}
