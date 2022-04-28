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

                Notification pm = new();
                
                List<Notification> unsent = await pm.GetUnsentNotifications();
                foreach (Notification m in unsent)
                {
                    if (m.PushClients == null)
                    {
                        continue;
                    }

                    foreach(PushClient client in m.PushClients)
                    {
                        if (client != null)
                        {
                            PushSubscription subscription = new()
                            {
                                Endpoint = client.Endpoint,
                                P256DH = client.P256dh,
                                Auth = client.Auth
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
                                url = "../../Messages",
                                sound = "../sounds/right.wav"
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

                            Console.WriteLine(DateTime.Now + " | Message sent " + m.Body + " " + client.UserId);
                        }
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