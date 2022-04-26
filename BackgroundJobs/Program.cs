using BackendData;
using WebPush;

namespace BackgroundJobs
{
    internal class Program
    {
        public static readonly string publicKey = "BPamIAYhRbA8FL7dSmPpVo1vv3StKcpAPVl6Xg_e2WiQKnfz-WvXBFheOL0Mxzwr2kmGAwPR1IrJXWOytqe9jWU";
        public static readonly string privateKey = "wwQoOT0ehNfFgtpUnaUhNEzuCtuIg-qlWO2DqHrP5zQ";

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }
        public static async Task MainAsync()
        {
            Console.WriteLine("Starting up...");
            await SendPushNotifications();
        }

        internal async static Task<bool> SendPushNotifications()
        {
            try
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString());

                var subject = @"mailto:arni.bjorgvinsson@marel.com";

                PushNotification pm = new();
                
                List<PushNotification> unsent = await pm.GetUnsentPushNotifications(pm.GetPushClient());
                foreach (PushNotification m in unsent)
                {
                    if (m.PushClient != null)
                    {
                        var pushEndpoint = m.PushClient?.Endpoint;
                        var p256dh = m.PushClient?.P256dh;
                        var auth = m.PushClient?.Auth;

                        PushSubscription subscription = new()
                        {
                            Endpoint = pushEndpoint,
                            P256DH = p256dh,
                            Auth = auth
                        };

                        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

                        MessagePayload mp = new()
                        {
                            body = m.Body,
                            subject = subject,
                            icon = m.Icon,
                            primarykey = m.Id.ToString(),
                            tag = m.Title,
                            category = "category",
                            subtitle = "sub-title",
                            url = "https://localhost:7174/Messages",
                            sound = null
                        };

                        var webPushClient = new WebPushClient();
                        try
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(mp));
                            await webPushClient.SendNotificationAsync(subscription, Newtonsoft.Json.JsonConvert.SerializeObject(mp), vapidDetails);
                        }
                        catch (WebPushException exception)
                        {
                            Console.WriteLine("Http STATUS code" + exception.StatusCode);
                        }
                        m.Sent = true;
                        //todo update message as sent

                        Console.WriteLine(DateTime.Now + " | Message sent " + m.Body + " " + m.PushClient?.UserName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }
    }
}