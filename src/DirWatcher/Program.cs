using DirWatcher.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var snapshotDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "snapshots");
builder.Services.AddSingleton<ISnapshotStore>(_ => new JsonSnapshotStore(snapshotDir));
builder.Services.AddSingleton<ChangeDetectionService>();

var demoPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "demo"));
builder.Services.AddSingleton(new DemoFolderService(demoPath));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
