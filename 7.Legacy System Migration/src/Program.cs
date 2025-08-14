using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Legacy Migration API", Version = "v1" });
});

// Add Feature Management
builder.Services.AddFeatureManagement();

// Register compatibility services
builder.Services.AddSingleton<ILegacyCompatibilityLayer, LegacyCompatibilityLayer>();

// Register old and new implementations
builder.Services.AddScoped<ILegacyAuthService, LegacyAuthService>();
builder.Services.AddScoped<IModernAuthService, ModernAuthService>();
builder.Services.AddScoped<IAuthenticationFactory, AuthenticationFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
