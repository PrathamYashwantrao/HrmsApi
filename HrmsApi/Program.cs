using HrmsApi.Data;
using HrmsApi.Repository;
using HrmsApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;


//--hasnain

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(Options => Options.UseSqlServer(builder.Configuration.GetConnectionString("dbconn")));
//builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddScoped<AuthRepo, AuthService>();
builder.Services.AddScoped<IOfferLetterService, OfferLetterService>();

builder.Services.AddScoped<taskInterface, TaskService>();


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin",
//        builder => builder
//            .WithOrigins("http://localhost:7134") // Replace with your local frontend URL
//            .AllowAnyHeader()
//            .AllowAnyMethod());
//});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HrmsApi v1"));
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
