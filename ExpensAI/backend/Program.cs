# .NET Core minimal API
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuração de CORS (Apenas origens permitidas)
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictCorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://meuapp.com" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Necessário para cookies HttpOnly
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Proteger Source Maps e Docs: Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Tratamento de Erros Seguro: Substitui StackTraces detalhados por mensagens genéricas em Produção
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            // Opcional: Aqui poderíamos buscar a exceção real com context.Features.Get<IExceptionHandlerFeature>()
            // e registrar num log centralizado (Serilog, Datadog), sem nunca expor ao frontend.
            
            await context.Response.WriteAsJsonAsync(new { 
                error = "Ocorreu um erro interno no servidor.", 
                message = "Nossa equipe já foi notificada. Tente novamente mais tarde." 
            });
        });
    });
}

// Middleware de Segurança: CSP (Content-Security-Policy) e Headers adicionais
app.Use(async (context, next) =>
{
    // Prevenir ataques XSS limitando as fontes de execução de scripts
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none';");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    await next();
});

app.UseCors("StrictCorsPolicy");
app.MapControllers();
app.Run();
