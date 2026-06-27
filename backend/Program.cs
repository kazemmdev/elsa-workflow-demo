using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using ElsaWorkflow.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var dbUrl = config.GetConnectionString("PostgreSql")!;

builder.Services.AddAppDatabase(dbUrl);
builder.Services.AddAppIdentity();
builder.Services.AddAppJwtTokenService(config);


builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(mg =>
    {
       mg.UseEntityFrameworkCore(ef =>
       {
            ef.UsePostgreSql(dbUrl);
            ef.RunMigrations = true;
       });
    });
    elsa.UseWorkflowRuntime(rt =>
    {
       rt.UseEntityFrameworkCore(ef =>
       {
            ef.UsePostgreSql(dbUrl);
            ef.RunMigrations = true;
       });
    });
    elsa.UseIdentity(idt =>
    {
        var identitySection = config.GetSection("Identity");

        idt.TokenOptions = opt =>
        {
            identitySection.GetSection("Tokens").Bind(opt);

            if (string.IsNullOrWhiteSpace(opt.SigningKey))
                opt.SigningKey = config["Jwt:SigningKey"]!;

            opt.Issuer   ??= "elsa-demo";
            opt.Audience ??= "elsa-demo-api";
        };

        idt.UseConfigurationBasedRoleProvider(opts => identitySection.Bind(opts));
    });
    elsa.UseDefaultAuthentication();
    elsa.UseWorkflowsApi();
    elsa.UseRealTimeWorkflows();
    elsa.UseHttp(http =>
    {
        http.ConfigureHttpOptions = opts =>
            opts.BaseUrl = new Uri(config["Http:BaseUrl"] ?? "http://localhost:5000");
    });
    elsa.UseJavaScript();
    elsa.UseLiquid();
    elsa.UseScheduling();
    elsa.AddActivitiesFrom<Program>();
    elsa.AddWorkflowsFrom<Program>();
});

builder.Services.AddElsaIdentityBridge();
builder.Services.AddDatabaseSeeder(config);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddCors(cors =>
    cors.AddDefaultPolicy(policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("x-elsa-workflow-instance-id")));

var app = builder.Build();

app.MigrateAndSeedDatabase();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); 
app.UseWorkflowsApi();   
app.UseWorkflows(); 
app.UseWorkflowsSignalRHubs();
app.MapHealthChecks("/health");

app.Run();
