using System.Text;
using MeAjudaAi.Application.Common;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Api.Filters;
using MeAjudaAi.Api.Middlewares;
using MeAjudaAi.Api.Swagger;
using MeAjudaAi.Api.Webhooks;
using MeAjudaAi.Infrastructure.DependencyInjection;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using MeAjudaAi.Application.Validators.Auth;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AdminMutationAuditFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<AdminMutationAuditFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerExamplesOperationFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Digite: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarUsuarioRequestValidator>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey) && builder.Environment.IsEnvironment("Testing"))
{
    jwtKey = "me-ajuda-ai-chave-de-teste-com-no-minimo-32-caracteres";
}

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key não configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "MeAjudaAi.Api",
            ValidAudience = jwtSection["Audience"] ?? "MeAjudaAi.Mobile",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IWebhookPagamentoPayloadAdapter, DefaultWebhookPagamentoPayloadAdapter>();
builder.Services.AddScoped<IWebhookPagamentoPayloadAdapter, AsaasWebhookPagamentoPayloadAdapter>();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new CampoErroValidacaoResponse
            {
                Campo = x.Key,
                Mensagens = x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            })
            .ToArray();

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new ErroValidacaoResponse
        {
            Mensagem = "Erro de validação.",
            Erros = erros
        });
    };
});

var app = builder.Build();

var desabilitarDbInitializer = builder.Configuration.GetValue<bool>("DesabilitarDbInitializer");

if (!desabilitarDbInitializer)
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InicializarAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseMiddleware<UsuarioAtivoMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
