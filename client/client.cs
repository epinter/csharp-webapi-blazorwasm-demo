using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

public class Client
{
    private static string? apiAudience = "";
    private static string? myClientId = "";
    private static string? authority = "";
    private static string? apiUrl = "";

    public static void Main()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();

        apiAudience = config["AzureAd:Audience"];
        myClientId = config["AzureAd:ClientId"];
        authority = config["AzureAd:Authority"];
        apiUrl = config["apiUrl"];

        Task.Run(async () =>
        {
            string json = await RequestApi();
            Console.WriteLine(json);
        }).GetAwaiter().GetResult();
    }

    private static async Task<string> RequestApi()
    {
        var scopes = new[] { apiAudience + "/access" };

        string accessToken = await SignInUserAndGetTokenUsingMSAL(scopes);
        Console.WriteLine("ACCESS TOKEN: " + accessToken);

        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Call the web API.
        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Signs in the user using the device code flow and obtains an Access token for MS Graph
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="scopes"></param>
    /// <returns></returns>
    private static async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
    {
        IPublicClientApplication application = PublicClientApplicationBuilder.Create(myClientId)
                                        .WithAuthority(authority)
                                        .WithDefaultRedirectUri()
                                        .Build();

        AuthenticationResult result;

        try
        {
            var accounts = await application.GetAccountsAsync();
            // Try to acquire an access token from the cache. If device code is required, Exception will be thrown.
            result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            result = await application.AcquireTokenWithDeviceCode(scopes, deviceCodeResult =>
               {
                   // This will print the message on the console which tells the user where to go sign-in using
                   // a separate browser and the code to enter once they sign in.
                   // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                   // device code callback to look for the successful login of the user via that browser.
                   // This background polling (whose interval and timeout data is also provided as fields in the
                   // deviceCodeCallback class) will occur until:
                   // * The user has successfully logged in via browser and entered the proper code
                   // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                   // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                   //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                   Console.WriteLine(deviceCodeResult.Message);
                   return Task.FromResult(0);
               }).ExecuteAsync();
        }
        return result.AccessToken;
    }

}