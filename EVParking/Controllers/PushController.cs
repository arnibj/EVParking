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
            public string p256dh { get; set; }
            public string auth { get; set; }
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
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("is-IS");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("is-IS");
                
                string user = PushClient.ReturnUserClaimTypeValue((ClaimsIdentity)User.Identity, "preferred_username");

                if (s != null && !string.IsNullOrEmpty(user))
                {
                    if (!s.ExpirationTime.HasValue)
                    {
                        s.ExpirationTime = DateTime.Now.AddYears(10);
                    }
                    Guid clientID = Guid.NewGuid();
                    PushClient p = new()
                    {
                        P256dh = s.Keys?.p256dh,
                        ExpirationTime = s.ExpirationTime,
                        Endpoint = s.Endpoint,
                        Auth = s.Keys?.auth,
                        UserName = user,
                        DateAdded = DateTime.Now,
                        ClientId = clientID
                    };
                    PushClient pushClient = new();
                    await pushClient.AddClient(p);
                    return clientID.ToString();
                }
            }
            catch (Exception ex)
            {
                
            }
            return String.Empty;
        }

        [HttpPost]
        [Route("api/push/unsubscribe")]
        public async Task<string> Unsubscribe(PushNotificationSubscription s)
        {
            PushClient p = new PushClient();
            var pushClient = await p.GetItems();
            var c = pushClient.FirstOrDefault(m => m.Auth == s.Keys?.auth && m.P256dh == s.Keys?.p256dh);
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
