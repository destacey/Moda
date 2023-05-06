using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moda.Web.BlazorClient;
using Moda.Web.BlazorClient.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientServices(builder.Configuration);
//builder.Services.AddHttpClient("GraphAPI",
//        client => client.BaseAddress = new Uri("https://graph.microsoft.com"))
//    .AddHttpMessageHandler<GraphAPIAuthorizationMessageHandler>();

await builder.Build().RunAsync();
