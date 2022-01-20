using EVParking.Models;
using EVParking.Settings;
using System.Text;

namespace EVParking
{
    public class tester
    {

        public async Task<bool> TestUserCreate()
        {
            var name = "arni";
            var email = "arni.bjorgvinsson@marel.com";
            //var isUserExists = await applicationDbContext.ApplicationUsers
            //    .AnyAsync(u => u.ObjectIdentifier == objectidentifier);
            User appUser = new User();
            appUser.Name = name;
            appUser.Email = email;
            appUser.Password = "123Abc.#";

			using (HttpClient c = new HttpClient())
			{
				c.Timeout = TimeSpan.FromSeconds(5);
				StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(appUser), Encoding.UTF8, "application/json");
				//c.BaseAddress = new Uri("https://localhost:7174/");
				//c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
				using (HttpResponseMessage response = c.PostAsync("https://localhost:7174/operations/create", content).Result)
				{
					response.EnsureSuccessStatusCode();
					string responseBody = response.Content.ReadAsStringAsync().Result;

					try
					{
						//FormProperties formProps = JsonConvert.DeserializeObject<FormProperties>(responseBody);
						
					}
					catch (Exception ex)
					{
						//manager.ErrorService.Save(ex, responseBody);
					}
				}
			}
			return true;
			//AppDbContext ctx = new AppDbContext();
			//return await ctx.CreateUser(appUser);
		}
    }
}
