using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blazorwasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

string? defaultTokenScope = builder.Configuration.GetValue<string>("DefaultTokenScope");
string apiUrl = builder.Configuration.GetSection("General").GetValue<string>("ApiUrl") ?? "";

if(defaultTokenScope == null) {
    builder.Services.AddMsalAuthentication(options =>
    {
        builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    });
} else {
    builder.Services.AddMsalAuthentication(options =>
    {
        builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes.Add(defaultTokenScope);
    });
}

builder.Services.AddOptions<AppSettings>().Configure<IConfiguration>((config, configuration) =>
{
    builder.Configuration.Bind("General", config);
});

builder.Services.AddTransient<CustomAuthorizationHandler>();

builder.Services.AddHttpClient("WebAPI",
        client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<CustomAuthorizationHandler>();

await builder.Build().RunAsync();
