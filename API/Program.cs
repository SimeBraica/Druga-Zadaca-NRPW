using API;
using API.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.WebHost.UseUrls("http://*:8080"); // Allow the app to listen on all IPs on port 8080

// Authentication services (JWT and Cookies)
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options => options.Cookie.Name = "token") // Cookie for storing JWT token
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Read from appsettings.json
        ValidAudience = builder.Configuration["Jwt:Audience"], // Read from appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Secret key for signing tokens
    };
    options.Events = new JwtBearerEvents {
        OnMessageReceived = context => {
            context.Token = context.Request.Cookies["token"]; // Get the JWT token from cookies
            return Task.CompletedTask;
        }
    };
});

// Register services
builder.Services.AddScoped<OwnerController>();
builder.Services.AddScoped<PetController>();
builder.Services.AddScoped<HttpClient>();

builder.Services.AddHttpContextAccessor();

// CORS configuration for frontend communication
string corsPolicyName = "cors";
builder.Services.AddCors(options => {
    options.AddPolicy(corsPolicyName, policy => {
        policy.WithOrigins("https://druga-zadaca-nrpw.onrender.com") // Allow only the specified frontend domain
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Enable credentials (cookies, authorization headers)
    });
});

// Database context setup
builder.Services.AddDbContext<NrppwDrugaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database_Conn")));

// Build and configure the app
var app = builder.Build();

// Middleware configuration
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting(); // Ensure routing is set up before authentication
app.UseCors(corsPolicyName); // Apply CORS policy
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

app.UseEndpoints(endpoints => {
    endpoints.MapControllers(); // Map API controllers
    endpoints.MapFallbackToFile("index.html"); // Serve the Blazor UI (SPA fallback)
});

app.Run(); // Start the API server
