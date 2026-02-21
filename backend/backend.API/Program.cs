using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        //.ReadFrom.Services(services)
        //.Enrich.FromLogContext()
        //.WriteTo.Console()
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
