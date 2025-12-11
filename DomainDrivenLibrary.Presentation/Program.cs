using DomainDrivenLibrary;
using DomainDrivenLibrary.Extensions;
using DomainDrivenLibrary.Middleware;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.ApplyMigrations();

// Exception handling middleware (must be early in pipeline)
app.UseExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();