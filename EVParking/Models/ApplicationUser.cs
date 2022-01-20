using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System.Text;

namespace EVParking.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public async Task<bool> AddUser(User user)
        {
			using (HttpClient c = new HttpClient())
			{
				c.Timeout = TimeSpan.FromSeconds(10);
                user.Password += "A";
				StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = c.PostAsync("https://localhost:7174/operations/create", content).Result;
                try
                {
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
		}

        public async Task<bool> LoginUser(string email, string password)
        {
            using (HttpClient c = new HttpClient())
            {
                c.Timeout = TimeSpan.FromSeconds(10);
                password += "A";
                User user = new User();
                user.Email = email;
                user.Name = email;
                user.Password = password;

                StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = c.PostAsync("https://localhost:7174/accounts/login2", content).Result;
                try
                {
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
