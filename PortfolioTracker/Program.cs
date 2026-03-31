using PortfolioTracker;
using PortfolioTracker.API.Middlewares;
using PortfolioTracker.Application;
using PortfolioTracker.Application.Interfaces;
using PortfolioTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddApplication();
builder.Services.AddCors(options =>
      options.AddPolicy("dev", policy =>
          policy.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()));
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.ConfigObject.AdditionalItems["withCredentials"] = true;
    });
    app.MapOpenApi();
}

app.UseCors("dev");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityEndpoints();
app.MapControllers();

app.Run();
