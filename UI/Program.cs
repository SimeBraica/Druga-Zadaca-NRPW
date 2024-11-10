using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI;
using UI.Services;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Captcha;
using Blazorise.Captcha.ReCaptcha;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add the root components (App and HeadOutlet)
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Blazorise and its providers
builder.Services
    .AddBlazorise(options => {
        options.Immediate = true;
    })
    .AddBootstrapProviders() // Adds Bootstrap to Blazorise
    .AddBlazoriseGoogleReCaptcha(reCaptchaOptions => {
        reCaptchaOptions.SiteKey = "6Lc1-HUqAAAAAOlwH177GLu1BPJN0O05ABebCtHd"; // ReCaptcha SiteKey
    });

// Register scoped services for login and pet-related functionality
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<PetService>();

// Configure the HttpClient with the base address for the API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://druga-zadaca-nrpw.onrender.com/") });
// Build and run the application
await builder.Build().RunAsync();
