using H4_Poker_Engine.Authentication;
using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add auth header to swagger!
// Makes us able to use Swagger documentation.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the bearer scheme (\"Bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // A filter added so swagger can figure out how to lock and unlock the tokens.
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// adds signalR as service
builder.Services.AddSignalR();

// this is adding the authentication schema to the application 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // Get's the key from Appsettings, and uses the key to sign the tokens.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["SecretKey"])),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/poker")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Background Service workers
builder.Services.AddHostedService<TableServiceWorker>();

// DependencyInjection
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddSingleton<BasePokerHub>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Test HTTPS connection when HTTP is running correctly.
//app.UseHttpsRedirection();

// Added authentication, make sure it is instantiated above authorization
// Won't authenticate if placed under UseAuthorization.
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// SignalR hubs mappings
app.MapHub<BasePokerHub>("/poker");

app.Run();