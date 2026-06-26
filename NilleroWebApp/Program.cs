using Nillero.Infrastructure.Identity.IOC;
using Nillero.Infrastructure.Persistence;
using Nillero.Core.Application.IOC;
using Nillero.Core.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Identity
builder.Services.AddIdentityInfrastructureLayerIoc(builder.Configuration);

// Repositories
builder.Services.AddPersistenceLayerIoc(builder.Configuration);

// Services
builder.Services.AddApplicationLayerIoc();

// Email 
builder.Services.AddSharedLayerIoc(builder.Configuration);

// Session
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Seeds
await app.Services.RunIdentitySeedAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();