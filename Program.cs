var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{

    options.AddDefaultPolicy(builderOptions =>
    {

        string[] origins = { "http://localhost:5173", "https://cecyt3prueba.ipn.mx" };

        builderOptions.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();

    });

});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/