using Microsoft.EntityFrameworkCore;
using MyApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Подключаем базу данных SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddControllers();

var app = builder.Build();

// СОЗДАЁМ БАЗУ, ЕСЛИ ЕЁ НЕТ (ВАЖНО!)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}

app.MapControllers();
app.Run();