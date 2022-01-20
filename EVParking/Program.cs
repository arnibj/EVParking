using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using EVParking.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using EVParking.Settings;

var builder = WebApplication.CreateBuilder(args);

MongoDbConfig mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
var azureSettings = builder.Configuration.GetSection(nameof(AzureAd)).Get<AzureAd>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        ValidateIssuer = false
    };

    options.Events = new OpenIdConnectEvents()
    {
        OnTicketReceived = async (context) =>
        {
            using (var scope = context.HttpContext.RequestServices.CreateScope())
            {
                var email = context.Principal.FindFirstValue("preferred_username");
                MongoUser user = new();
                ApplicationUser u = new();
                bool userExists = await user.DoesUserExist(email);
                if (!userExists)
                {
                    User appUser = new();
                    appUser.Name = email;
                    appUser.Email = email;
                    appUser.Password = azureSettings.ClientId;

                    bool userAdded = await u.AddUser(appUser);
                }
            };
        },
        OnTokenValidated = context =>
        {
            var idToken = context.SecurityToken;
            string userIdentifier = idToken.Subject;
            //string userEmail =
            //    idToken.Claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
            //    ?? idToken.Claims.SingleOrDefault(c => c.Type == "preferred_username")?.Value;

            //string firstName = idToken.Claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
            //string lastName = idToken.Claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
            //string name = idToken.Claims.SingleOrDefault(c => c.Type == "name")?.Value;

            //// manage roles, modify token and claims etc.

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.Redirect("/Home/Error");
            context.HandleResponse(); // Suppress the exception
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
})
.AddMvcOptions(options => { })
.AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

