using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace AndNet.Manager.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddAuthorizationCore();

        Uri baseUri = new(builder.HostEnvironment.BaseAddress);
        builder.Services.AddHttpClient("AndNet.Manager.Server", httpClient => httpClient.BaseAddress = baseUri);
        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("AndNet.Manager.Server"));

        builder.Services.AddScoped<AuthenticationStateProvider, AndNetAuthenticationStateProvider>();

        await builder.Build().RunAsync();
    }
}