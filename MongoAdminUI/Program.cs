using MongoAdminUI.Services;

var builder = WebApplication.CreateBuilder(args);


var mongoIDConnectionString = builder.Configuration.GetConnectionString("MongoIDDatabase");
var chemoMetecConnectionString = builder.Configuration.GetConnectionString("ChemoMetecDatabase");

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<RoleService>();
builder.Services.AddSingleton<PolicyService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set a timeout for the session
    options.Cookie.HttpOnly = true; // Set the session cookie to HttpOnly for security
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
