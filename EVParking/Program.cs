using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using EVParking.Models;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

EVParking.Settings.MongoDbConfig? mongoDbSettings = builder.Configuration.GetSection(nameof(EVParking.Settings.MongoDbConfig))
                                                                         .Get<EVParking.Settings.MongoDbConfig>();
var azureSettings = builder.Configuration.GetSection(nameof(EVParking.Settings.AzureAd)).Get<EVParking.Settings.AzureAd>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

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
                //TODO - Check if user exists otherwise add it

                //var applicationDbContext = scope.ServiceProvider.GetRequiredService<MongoDB>();
                //var name = context.Principal.FindFirstValue("name");
                //var email = context.Principal.FindFirstValue("preferred_username");
                //var isUserExists = await applicationDbContext.ApplicationUsers
                //    .AnyAsync(u => u.ObjectIdentifier == objectidentifier);
                //if (!isUserExists)
                //{
                //    User appUser = new User();
                //    appUser.Name = name;
                //    appUser.Email = email;
                //    appUser.Password = azureSettings.ClientId;
                //    var applicationUser = new ApplicationUser()
                //    {
                //        Email = email,
                //        UserName = name
                //    };
                //    //OperationsController op = new OperationsController(userManager, roleManager);
                //    //IdentityResult restult = await op.Create(appUser);
                //    //IdentityResult result = await userManager.CreateAsync(appUser, appUser.Password);
                //    //applicationDbContext.ApplicationUsers.Add(applicationUser);
                //    //await applicationDbContext.SaveChangesAsync();
                //};
            }
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

