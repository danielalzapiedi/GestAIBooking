using GestAI.Web;
using GestAI.Web.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var culture = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// ------------------------------------------------------------
// HttpClient "Front" (para assets / host base)
// ------------------------------------------------------------
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ------------------------------------------------------------
// Auth / Storage
// ------------------------------------------------------------
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>(); // ✅ ahora con Get/Set/Clear
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<LocalStorageService>(); // si lo usás en otras cosas
builder.Services.AddSingleton<AppState>();

// ------------------------------------------------------------
// Handlers (solo para BackendApi)
// ------------------------------------------------------------
builder.Services.AddTransient<AttachTokenAndGuardHandler>();
builder.Services.AddTransient<Redirect401Handler>();

// ------------------------------------------------------------
// HttpClient "BackendApi" (el que usa ApiClient)
// ------------------------------------------------------------
builder.Services.AddHttpClient("BackendApi", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    // ✅ En WASM, CreateDefault ya carga wwwroot/appsettings*.json
    // Fallback dev para que no explote si falta la key
    var baseUrl = cfg["Backend:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        baseUrl = "http://localhost:5071/";

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
})
.AddHttpMessageHandler<AttachTokenAndGuardHandler>()
.AddHttpMessageHandler<Redirect401Handler>();

// ------------------------------------------------------------
// ApiClient usando BackendApi (✅ ya NO recibe JwtAuthStateProvider)
// ------------------------------------------------------------
builder.Services.AddScoped<ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var http = factory.CreateClient("BackendApi");
    return new ApiClient(http);
});

await builder.Build().RunAsync();
