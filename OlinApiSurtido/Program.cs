using MiApi.Context;
using MiApi.Interfaces;
using MiApi.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction()) { builder.WebHost.UseUrls("http://192.168.0.118:5000", "https://192.168.0.118:5000"); }

var connectionString = builder.Configuration.GetConnectionString("OlinCeConnection");
builder.Services.AddDbContext<Contexto>(options => options.UseSqlServer(connectionString));


// Add services to the container.
builder.Services.AddScoped<IRepositorioDocumentoDetalle, RepositorioDocumentoDetalle>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
