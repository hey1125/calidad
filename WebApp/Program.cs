var builder = WebApplication.CreateBuilder(args);

// 1 Servicios
builder.Services.AddRazorPages();

var app = builder.Build();

// 2 Middleware estándar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 3 Redirección de "/" → "/Public/Index"
app.MapGet("/", context =>
{
    context.Response.Redirect("/Public/Index");
    return Task.CompletedTask;
});

// 4 Razor Pages
app.MapRazorPages();

app.Run();
