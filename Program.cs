using awt_pj_ss23_green_streaming_1.Hubs;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// it is needed to serve the static files with unkown types such as mpd
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { { ".mpd", "application/dash+xml" } }
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<MeasurementHub>("/measurementHub");
app.MapHub<PlaybackHub>("/playbackHub");
app.MapHub<PythonScriptHub>("/pythonScriptHub");

app.Run();
