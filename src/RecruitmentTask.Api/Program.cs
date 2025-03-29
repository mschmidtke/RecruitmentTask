using System.Text;
using RecruitmentTask.Api.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddBackgroundExchangeRateUpdater();
builder.Services.AddWalletServices();
builder.Services.AddSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

app.Run();
