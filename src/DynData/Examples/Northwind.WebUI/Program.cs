using Northwind.DataAccess;
using Northwind.WebUI.Configuration;
using a2n.DynData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var settings = new AppSettings();
builder.Configuration.Bind("AppSettings", settings);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// Add services to the container.
var pages = builder.Services.AddRazorPages();


// Important!! dynData required property MemberCasing as name of Property in C# class
//pages.AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.PropertyNamingPolicy = null;
//});
pages.AddNewtonsoftJson(x =>
{
    x.UseMemberCasing();
    x.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
});

builder.Services.AddDbContext<NorthwindDbContext>(o =>
{
    // without custom event handler
    //o.UseDynData(settings.DBConnectionSetting);

    // with custom event handler
    o.UseDynData(settings.DBConnectionSetting);
});
builder.Services.AddDynDataApi<NorthwindDbContext, NorthwindQueryTemplate>("db");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.Run();
