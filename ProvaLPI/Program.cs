using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using ProvaLPI.Service;
using ProvaLPI;

#region Serilog
var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
Directory.CreateDirectory(logFolder);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(new CompactJsonFormatter(),
          Path.Combine(logFolder, ".json"),
          retainedFileCountLimit: 3,
          rollingInterval: RollingInterval.Day)
    .WriteTo.File(Path.Combine(logFolder, ".log"),
          retainedFileCountLimit: 3,
          rollingInterval: RollingInterval.Day)
    .CreateLogger();
#endregion

var builder = WebApplication.CreateBuilder(args);

#region JWT

builder.Services
    .AddAuthentication(x =>
    {
        //Especificando o Padr�o do Token

        //para definir que o esquema de autentica��o que queremos utilizar � o Bearer e o
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        //Diz ao asp.net que utilizamos uma autentica��o interna,
        //ou seja, ela � gerada neste servidor e vale para este servidor apenas.
        //N�o � gerado pelo google/fb
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(x =>
    {
        //Lendo o Token

        // Obriga uso do HTTPs
        x.RequireHttpsMetadata = false;

        // Configura��es para leitura do Token
        x.TokenValidationParameters = new TokenValidationParameters
        {
            // Chave que usamos para gerar o Token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("minha-chave-secreta-minha-chave-secreta")),
            ValidAudience = "Usu�rios da API",
            ValidIssuer = "Unoeste",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

//pol�tica
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("APIAuth", new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser().Build());
});
#endregion

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Habilitar o uso do serilog.
builder.Host.UseSerilog();

#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gerenciamento da API...",
        Version = "v1",
        Description = "Prova 1� Bimestre - Linguagens de Programa��o I",
        Contact = new OpenApiContact
        {
            Name = "Suporte Unoeste",
            Email = string.Empty,
            Url = new Uri("https://www.unoeste.br"),
        },
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira o token JWT no formato *Bearer {token}*",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] { }
        }
    });



    // Adiciona a seguran�a ao Swagger
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
            new string[] { }
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});
#endregion

#region IOC
//adicionado ao IOC por requisi��o
builder.Services.AddScoped(typeof(UsuarioService));
builder.Services.AddScoped(typeof(PerfilService));
builder.Services.AddScoped(typeof(CategoriaService));

builder.Services.AddSingleton<BD>(new BD());
#endregion

#region Lendo o appsettings
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

string pathAppsettings = "appsettings.json";

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(pathAppsettings)
    .Build();

Environment.SetEnvironmentVariable("stringConexao", config.GetSection("stringConexao").Value);
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// *** Usa o Middleware do Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.RoutePrefix = ""; //habilitar a p�gina inicial da API ser a doc.
    c.DocumentTitle = "Gerenciamento de Produtos - API V1";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
