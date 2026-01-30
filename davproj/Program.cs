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
builder.Services.AddHttpClient();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();