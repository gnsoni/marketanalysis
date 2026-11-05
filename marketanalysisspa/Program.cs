// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts => {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Only enable HTTPS redirect and HSTS when an HTTPS listener is actually configured.
// In the container behind nginx, Kestrel only sees plain HTTP (nginx terminates TLS),
// so redirecting to HTTPS would produce a 307 → dead port → timeout.
// Set ASPNETCORE_HTTPS_PORT (e.g. "8443") to opt back in when running HTTPS directly.
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
