var builder = WebApplication.CreateBuilder(args);

// Tambahkan layanan ke container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Mengaktifkan generator Swagger

var app = builder.Build();

// Konfigurasi HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Mengaktifkan endpoint swagger.json
    app.UseSwaggerUI(); // Mengaktifkan tampilan UI Swagger
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();