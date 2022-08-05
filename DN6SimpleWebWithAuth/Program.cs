//using Azure.Identity;
using DN6SimpleWebWithAuth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

// OPTIONALLY: enable automatic migrations
// *************
// NOTE: IF you do this, you will never be able to roll-back a migration with this code in place.  Use at your own discretion:
// **************
/*
//automatically apply database migrations (breaks solution if database not wired up correctly, forces roll-forward approach
var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
using (var context = new ApplicationDbContext(contextOptions))
{
    context.Database.Migrate();
}
*/

/* 
*************************** 
Redis Cache Integration
***************************
*/
//Uncomment this section to integrate with Redis Cache
/*
string redisCNSTR = string.Empty;
var env = builder.Configuration["Application:Environment"];

if (string.IsNullOrWhiteSpace(env) || !env.Equals("develop", StringComparison.OrdinalIgnoreCase))
{
    redisCNSTR = builder.Configuration["REDIS_CONNECTION_STRING"];
}
else
{
    var redisSection = builder.Configuration.GetSection("Redis");
    redisCNSTR = redisSection.GetValue<string>("ConnectionString").ToString();
    var redisInstanceName = redisSection.GetValue<string>("InstanceName");
}

//Turn this on to use session data in Redis and avoid using cookies for logins
////session
//builder.Services.AddSession(o =>
//{
//    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    o.Cookie.Name = "DemoRedis.Session";
//    o.Cookie.HttpOnly = true;
//});

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString(redisCNSTR);
//    options.InstanceName = redisInstanceName;
//});

//Direct access to the cache
builder.Services.AddSingleton(async x => await RedisConnection.InitializeAsync(connectionString: redisCNSTR));
*/

/* 
 * App Configuration and KeyVault integration: 
 *      To enable the use of AppConfiguration and Keyvault integration, uncomment the following code, using the version of credential without the keyvalt configured
 *      - ENSURE you have a system managed identity for your App Service(s)
 *      - Add the default url for the Azure App Config as a configuration variable 
 *          i.e. 
 *              AzureAppConfigConnection 
 *              https://someazureappconfignamehere.azconfig.io
 *      - For the developer, add the secret in your secret config but use the full connection string to the azure app configuration
 *      - Add all of the app service(s) and slots to the App Configuration with Configuration Data Reader Role
 *      - Add your developer to the app service with data reader role
 *
 * Additional NOTE: If using KeyVault, 
 *      - ENSURE you have a system managed identity for your App Service(s) and your Azure App COnfig
 *      - ENSURE you have an access policy on your key vault for the Azure App Config and your App Service(s)
 *      - You will also need to switch the config generation to the keyvault version for each environment below.
 *      - Finally, note that your devs will also need access to keyvault get secret in order to work with this locally through the default credential
 * 
 * Final Note: For the code to work, ensure you have NuGet packages for Azure.Identity and Microsoft.Extensions.Configuration.AzureAppConfiguration (they are already added to this project)
 *
*/

/*
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var settings = config.Build();
    var env = settings["Application:Environment"];
    if (env == null || !env.Trim().Equals("develop", StringComparison.OrdinalIgnoreCase))
    {
        var cred = new ManagedIdentityCredential();
        config.AddAzureAppConfiguration(options =>
                options.Connect(new Uri(settings["AzureAppConfigConnection"]), cred));

        //config.AddAzureAppConfiguration(options =>
        //    options.Connect(settings["AzureAppConfigConnection"])
        //                    .ConfigureKeyVault(kv => { kv.SetCredential(cred); }));                                  
        
    }
    else
    {
        var cred = new DefaultAzureCredential();
        config.AddAzureAppConfiguration(options =>
            options.Connect(settings["AzureAppConfigConnection"]));
        
        //config.AddAzureAppConfiguration(options =>
        //    options.Connect(settings["AzureAppConfigConnection"])
        //                    .ConfigureKeyVault(kv => { kv.SetCredential(cred); }));
        
    }
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
