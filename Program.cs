// Program.cs (Final Version)
using SecureCommECC.Services;
using SecureCommECC.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// *** CRITICAL FIX: Change from Singleton to Transient ***
// This ensures a new, fresh service is created for every API request,
// guaranteeing that "Generate Keys" is always random.
builder.Services.AddTransient<ECCService>();

builder.Services.AddSingleton<AIService>();
builder.Services.AddSignalR();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();