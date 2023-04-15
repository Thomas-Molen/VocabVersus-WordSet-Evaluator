using vocabversus_wordset_evaluator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to DI.
builder.Services.AddSingleton<WordSetService>();

// Add webAPI controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors(builder =>
{
    builder.AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed((host) => true)
        .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
