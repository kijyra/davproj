using davproj.Filters;
using davproj.Models;
using davproj.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJSApp", policy =>
    {
        policy.WithOrigins("https://10.0.0.70", "https://dc1.dallari.biz", "https://localhost", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseNpgsql(dataSource);
});
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();
builder.Services.AddScoped<UserSettingsService>();
builder.Services.AddAuthorization(options => { });
builder.Services.AddRazorPages();
builder.Services.AddScoped<IKyoceraSnmpService, KyoceraSnmpService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddHttpClient();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseRouting();
app.UseCors("NextJSApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();