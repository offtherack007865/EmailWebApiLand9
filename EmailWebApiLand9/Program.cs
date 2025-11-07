using log4net;
using log4net.Config;

using Microsoft.AspNetCore.Builder;

using Microsoft.EntityFrameworkCore;

using EmailWebApiLand9.Data.Models;
using EmailWebApiLand9.Data;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

ILog log = LogManager.GetLogger(typeof(Program));


// configure logging via log4net

string log4netConfigFullFilename =
    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log4net.config");

var fileInfo = new FileInfo(log4netConfigFullFilename);
if (fileInfo.Exists)
    log4net.Config.XmlConfigurator.Configure(fileInfo);
else
    throw new InvalidOperationException("No log config file found");

log.Info($"Started EmailWebApi9 Program");

var builder = WebApplication.CreateBuilder(args);

// Add service and create Policy with options
builder.Services.AddCors(options =>
    options.AddPolicy("MyPolicy", builder =>
    {
        builder.WithOrigins("http://webservices:4042", "http://localhost:4200", "http://localhost:5003")
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader();
    }));

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

string projectPath = AppDomain.CurrentDomain.BaseDirectory;
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(projectPath)
    .AddJsonFile(MyConstants.AppSettingsFile)
    .Build();


//Data Source=sqldata; Initial Catalog=Production Monitoring; Integrated Security=False; User ID=dwproduction;Password=dwproduction
var withEfConnection = configuration.GetConnectionString(MyConstants.ConnectionString);
builder.Services.AddDbContextPool<EmailWebApi8Context>
    (options => options.UseSqlServer(withEfConnection));

builder.Services.AddControllers()
    .AddNewtonsoftJson();

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
app.UseRouting();

app.UseCors("MyPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
