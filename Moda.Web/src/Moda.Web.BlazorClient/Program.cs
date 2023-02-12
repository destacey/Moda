using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moda.Web.BlazorClient;
using Moda.Web.BlazorClient.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientServices(builder.Configuration);

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

//    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://fdca5e6f-46a2-455c-b2f3-06a9a6877190/access_as_user");

//    options.ProviderOptions.LoginMode = "redirect";
//});

//builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
