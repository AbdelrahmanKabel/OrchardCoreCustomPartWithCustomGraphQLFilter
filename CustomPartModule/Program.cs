var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
//builder.Services.AddControllers();
builder.Services.AddMvcCore();
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

app.UseAuthorization();

app.MapRazorPages();
//app.MapControllers();
app.UseMvc();
app.MapAreaControllerRoute(
        name: "Home",
        areaName: "CustomPartModule",
        pattern: "",
        defaults: new { controller = "SurveyResults", action = "Index" }
    );
app.Run();
