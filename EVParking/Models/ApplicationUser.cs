using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System.Text;

namespace EVParking.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        /// <summary>
        /// Adds user to MongoDB instance
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="root">API Base URI</param>
        /// <returns></returns>
        public async Task<bool> AddUser(User user, string root)
        {
            using HttpClient c = new();
            c.Timeout = TimeSpan.FromSeconds(10);
            c.BaseAddress = new Uri(root);
            user.Password += "A";
            StringContent content = new(Newtonsoft.Json.JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = c.PostAsync("/operations/create", content).Result;
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

        /// <summary>
        /// Login user using email, password and base api uri
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="password">Password</param>
        /// <param name="root">BaseURI</param>
        /// <returns></returns>
        public async Task<bool> LoginUser(string email, string password, string root)
        {
            using HttpClient c = new HttpClient();
            c.BaseAddress = new Uri(root);
            c.Timeout = TimeSpan.FromSeconds(10);
            password += "A";
            User user = new();
            user.Email = email;
            user.Name = email;
            user.Password = password;

            StringContent content = new(Newtonsoft.Json.JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = c.PostAsync("/accounts/login2", content).Result;
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
