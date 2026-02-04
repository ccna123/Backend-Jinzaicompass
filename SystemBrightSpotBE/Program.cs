



global using SystemBrightSpotBE.Base;
global using SystemBrightSpotBE.Data;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json.Serialization;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Providers;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.CategoryService;
using SystemBrightSpotBE.Services.CompanyService;
using SystemBrightSpotBE.Services.ContactServices;
using SystemBrightSpotBE.Services.CronJobService;
using SystemBrightSpotBE.Services.DashboardService;
using SystemBrightSpotBE.Services.ExcelSeederService;
using SystemBrightSpotBE.Services.Hosted;
using SystemBrightSpotBE.Services.MonitoringSystemService;
using SystemBrightSpotBE.Services.NotificationService;
using SystemBrightSpotBE.Services.OrganizationService;
using SystemBrightSpotBE.Services.PermissionService;
using SystemBrightSpotBE.Services.PlanService;
using SystemBrightSpotBE.Services.ReportService;
using SystemBrightSpotBE.Services.S3Service;
using SystemBrightSpotBE.Services.SettingService;
using SystemBrightSpotBE.Services.SkillService;
using SystemBrightSpotBE.Services.TenantService;
using SystemBrightSpotBE.Services.UserPlanConditionService;
using SystemBrightSpotBE.Services.UserPlanService;
using SystemBrightSpotBE.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// SSM PASSWORD RETRIEVAL
// =======================================================
var ssmClient = new AmazonSimpleSystemsManagementClient();

var dbHost = builder.Configuration["DB_HOST"];
var dbName = builder.Configuration["DB_NAME"];
var dbUser = builder.Configuration["DB_USER"];
var passwordParam = builder.Configuration["DB_PASSWORD_PARAM"];

string dbPassword;

try
{
    var response = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = passwordParam,
        WithDecryption = true
    });

    dbPassword = response.Parameter.Value;
}
catch (ParameterNotFoundException)
{
    throw new Exception($"[Config Error] Missing SSM parameter: {passwordParam}");
}

var conn =
    $"Host={dbHost};Port=5432;Database={dbName};Username={dbUser};Password={dbPassword}";

//=========================================
// CONFIGURATION CORS
//=========================================
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

//=========================================
// CONFIGURATION SWAGGER (API Documentation)
//=========================================
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SystemBrightSpotBE", Version = "v1" });
    //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = """Standard Authorization header using the Bearer scheme. Example: "Bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.SchemaFilter<EnumSchemaFilter>();
});

//=========================================
// CONFIGURATION AUTHENTICATION (JWT)
//=========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration.GetSection("AppSettings:Token").Value!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var userId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = context.Principal!.FindFirstValue(ClaimTypes.Role);
                var userPassword = context.Principal!.FindFirstValue("Password");

                if (!String.IsNullOrEmpty(userId))
                {
                    long? roleId = await userService.FindRoleByUserId(long.Parse(userId));
                    string? password = await userService.FindPasswordByUserId(long.Parse(userId));

                    if (roleId == null || roleId != long.Parse(userRole!))
                    {
                        context.Fail("User role has changed. Token is no longer valid.");
                    }

                    if (String.IsNullOrEmpty(password) || userPassword != password)
                    {
                        context.Fail("User password has changed. Token is no longer valid.");
                    }
                }
                else
                {
                    context.Fail("Invalid token");
                }
            }
        };
    });

//=========================================
// DEPENDENCY INJECTION
//=========================================
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<DataContext>(
    o => o.UseNpgsql(conn)
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IExcelSeederService, ExcelSeederService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IUserPlanService, UserPlanService>();
builder.Services.AddScoped<IUserPlanConditionService, UserPlanConditionService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IMonitoringSystemService, MonitoringSystemService>();


//=========================================
// SESSION & CACHE
//=========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { });

//=========================================
// API BEHAVIOR
//=========================================
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//=========================================
// Init LOG SERVICE
//=========================================
builder.Services.AddSingleton<LogInitializer>();
builder.Services.AddHostedService<LogInitializerHostedService>();

//=========================================
// Init CronJob SERVICE
//=========================================
builder.Services.AddSingleton<IHostedService, TenantCronJobService>();


//=========================================
// Intergrate to AWS Lambda
//=========================================
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

//=========================================
// BUILD APP
//=========================================
var app = builder.Build();

//=========================================
// CONFIGURE HTTP REQUEST PIPELINE
//=========================================
var useSwagger = builder.Configuration.GetValue<bool>("AppSettings:UseSwagger");
if (useSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//=========================================
// APPLICATION STARTUP SERVICE SCOPE
//=========================================
using (var scope = app.Services.CreateScope())
{
    // =========================================
    // AUTO MIGRATE DATABASE
    // =========================================
    var migrate = builder.Configuration.GetValue<bool>("Database:Migrate");

    if (migrate)
    {
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        db.Database.Migrate();
    }

    //=========================================
    // AUTO SEEDER DATA MASTER
    //=========================================
    var seederService = scope.ServiceProvider.GetRequiredService<IExcelSeederService>();
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

    var pathToFileCategory = "./Data/Database/Seeder/category.xlsx";
    var pathToFileUser = "./Data/Database/Seeder/user.xlsx";
    var seeder = builder.Configuration.GetValue<bool>("Database:Seeder");

    if (seeder)
    {
        await seederService.SeedDataFromExcelAsync(pathToFileCategory, pathToFileUser);
    }
}

//=========================================
// Exposed to the outside to call backend in AWS
//=========================================
builder.WebHost.UseUrls("http://0.0.0.0:5120");

//=========================================
// システム時刻が UTC に変換されていません
//=========================================
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDefaultFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();