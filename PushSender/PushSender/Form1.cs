using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebPush;

namespace PushSender
{
   
    public partial class Form1 : Form
    {
        //https://web-push-codelab.glitch.me/
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessages();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sendMessages();   
        }

        protected void sendMessages()
        {
            try
            {
                listBox1.Items.Add(DateTime.Now.ToLongTimeString());
                
                var subject = @"TestMessage";
                var publicKey = txtPublic.Text;
                var privateKey = txtPrivate.Text;
                PushMessage pm = new PushMessage();
                List<PushMessage> unsent = (from m in pm.GetUnsentPushMessages() orderby m.SentDate select m).ToList();
                foreach (PushMessage m in unsent)
                {
                    var pushEndpoint = m.PushClient.Endpoint;
                    var p256dh = m.PushClient.p256dh;
                    var auth = m.PushClient.auth;

                    var subscription = new PushSubscription(pushEndpoint, p256dh, auth);
                    var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

                    MessagePayload mp = new MessagePayload
                    {
                        body = m.Body,
                        subject = m.Title,
                        icon = m.Icon,
                        primarykey = m.Id.ToString(),
                        tag = "tag"
                    };

                    var webPushClient = new WebPushClient();
                    try
                    {
                        webPushClient.SendNotificationAsync(subscription, Newtonsoft.Json.JsonConvert.SerializeObject(mp), vapidDetails);
                        //webPushClient.SendNotificationAsync(subscription, m.Body, vapidDetails);
                    }
                    catch (WebPushException exception)
                    {
                        listBox1.Items.Add("Http STATUS code" + exception.StatusCode);
                    }
                    m.Sent = true;
                    //todo update message as sent

                    listBox1.Items.Add(DateTime.Now + " | Message sent " + m.Body);
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1_Tick(sender, e);
        }
    }
}
