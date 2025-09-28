using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using nfse_backend.Data;
using nfse_backend.Mappings;
using nfse_backend.Repositories;
using nfse_backend.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using nfse_backend.Services.Pdf;
using static QuestPDF.Settings;
using QuestPDF.Infrastructure;
using nfse_backend.Services.Xml;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Impostos;
using nfse_backend.Services.Eventos;
using nfse_backend.Services.Configuracao;
using nfse_backend.Services.Armazenamento;
using nfse_backend.Services.NotaFiscal;
using nfse_backend.Services.NFe;
using nfse_backend.Services.Monitoramento;
using nfse_backend.Services.Validation;
using nfse_backend.Services.Calculation;
using nfse_backend.Services.Audit;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Manter nomes originais
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<NfsePdfService>();

// Serviços NF-e
builder.Services.AddSingleton<XmlValidationService>();
builder.Services.AddSingleton<XmlGeneratorService>();
builder.Services.AddSingleton<CertificadoDigitalService>();
builder.Services.AddScoped<SefazWebServiceClient>();
builder.Services.AddSingleton<CalculoImpostosService>();
builder.Services.AddSingleton<TabelasImpostosService>();
builder.Services.AddScoped<EventosNFeService>();
builder.Services.AddScoped<DanfeService>();
builder.Services.AddScoped<Danfe2026Service>();
builder.Services.AddSingleton<ConfiguracaoNFeService>();

// Serviços NF-e 2026
builder.Services.AddSingleton<XmlGenerator2026Service>();
builder.Services.AddScoped<NFe2026ValidationService>();
builder.Services.AddScoped<NFe2026CalculationService>();
builder.Services.AddScoped<NFe2026AuditService>();
builder.Services.AddScoped<NFe2026Service>();
builder.Services.AddScoped<ArmazenamentoSeguroService>();
builder.Services.AddScoped<NFeService>();
builder.Services.AddScoped<ContingenciaNFeService>();
builder.Services.AddScoped<AuditoriaService>();

// Serviços de monitoramento
builder.Services.AddSingleton<MonitoramentoService>();
builder.Services.AddHostedService<MonitoramentoService>(provider => provider.GetRequiredService<MonitoramentoService>());

// HttpClient com Polly para resiliência
builder.Services.AddHttpClient<SefazWebServiceClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.ASCII.GetBytes("chave_super_secreta_nfse2026");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.UseStaticFiles();

app.MapControllers();

app.Run();

// Make the implicit Program class accessible to integration tests
public partial class Program { }