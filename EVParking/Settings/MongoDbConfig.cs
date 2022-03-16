using EVParking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EVParking.Settings
{
    public class MongoDbConfig
    {
        public string? Name { get; init; }
        public string? Host { get; init; }
        public int Port { get; init; }
        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }

    public class AppDbContext
    {
        private UserManager<MongoUser> userManager;

        public IMongoDatabase MongoDatabase { get; set; }
        public AppDbContext(UserManager<MongoUser> userManager)
        {
            MongoDbConfig config = new();
            var client = new MongoClient(config.ConnectionString);
            MongoDatabase = client.GetDatabase(config.Name);
            this.userManager = userManager;
        }

        public async Task<bool> CreateUser(User user)
        {
            MongoUser appUser = new()
            {
                Name = user.Name,
                Email = user.Email
            };

            IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
            if (result.Succeeded)
                return true;
            else
                return false;
        }
    }
}
