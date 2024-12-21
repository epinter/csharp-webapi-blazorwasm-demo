using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

public class CustomAuthorizationHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationHandler(IAccessTokenProvider provider, 
        NavigationManager navigation, IOptions<AppSettings> options)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: [ options.Value.ApiUrl ],
            scopes: [ options.Value.HandlerAuthScope ]);
    }
}