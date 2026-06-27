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

app.MapPost("/api/scan", (string? path, ChangeDetectionService service) =>
{
    if (string.IsNullOrWhiteSpace(path))
        return Results.BadRequest(new { error = "Query parameter 'path' is required." });

    try
    {
        return Results.Ok(service.Analyze(path));
    }
    catch (DirectoryNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
