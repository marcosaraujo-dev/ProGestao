using ProGestao.Configuration;
using ProGestao.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Configurar fontes de configuração (MANTIDO - sua implementação original)
 //   builder.Configuration
 //       .SetBasePath(Directory.GetCurrentDirectory())
 //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
 //       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
 //       .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
 //       .AddEnvironmentVariables()
 //       .AddUserSecrets<Program>(optional: true);

    // Adicionar serviços com configuração segura
    builder.Services.AddRazorPages();
    // builder.Services.AddSecureConfiguration(builder.Configuration);

    // Configuração personalizada de aplicação e DbContext
    builder.Services.AddApplicationServices(builder.Configuration);

  //  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
  //
  //  // Logging 
  //  builder.Services.AddLogging(logging =>
  //  {
  //      logging.ClearProviders();
  //      logging.AddConsole();
  //      logging.AddDebug();
  //
  //      if (builder.Environment.IsDevelopment())
  //      {
  //          logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
  //      }
  //  });
  //
  //  // DbContextPool com configurações de resilência resolvendo problemas de concorrência
  //  builder.Services.AddDbContextPool<ProGestaoContext>(options =>
  //  {
  //      options.UseSqlServer(connectionString, sqlServerOptions =>
  //      {
  //          // Configurações de resilência
  //          sqlServerOptions.EnableRetryOnFailure(
  //              maxRetryCount: 3,
  //              maxRetryDelay: TimeSpan.FromSeconds(5),
  //              errorNumbersToAdd: null);
  //
  //          // Timeout para consultas
  //          sqlServerOptions.CommandTimeout(30);
  //      });
  //
  //      // ✅ CONFIGURAÇÃO ESPECÍFICA PARA RESOLVER O ERRO
  //      // Remove o tracking para melhor performance
  //      options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  //
  //      // Configurações de desenvolvimento
  //      if (builder.Environment.IsDevelopment())
  //      {
  //          options.EnableSensitiveDataLogging();
  //          options.EnableDetailedErrors();
  //      }
  //  },
  //  poolSize: 64);  // ✅ POOL SIZE para resolver concorrência
  //
  //  builder.Services.AddApplicationServices();  // Chama método do DependencyInjectionConfig


    var app = builder.Build();

    // Configuração do pipeline de middleware
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

   // using (var scope = app.Services.CreateScope())
   // {
   //     var context = scope.ServiceProvider.GetRequiredService<ProGestaoContext>();
   //     // Verifica se o contexto foi criado corretamente
   //     _ = context.Database.ProviderName; // Força verificação
   // }
   //
   // // Testar conexão com banco na inicialização
   // if (app.Environment.IsDevelopment())
   // {
   //     var canConnect = await ConfigurationValidator.TestDatabaseConnectionAsync(connectionString!);
   //
   //     if (!canConnect)
   //     {
   //         app.Logger.LogWarning("⚠️  Não foi possível conectar ao banco de dados. Verifique a connection string.");
   //     }
   //     else
   //     {
   //         app.Logger.LogInformation("✅ Conexão com banco de dados validada com sucesso!");
   //     }
   // }

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();
    app.MapRazorPages();

    // Configurar pipeline com segurança
    app.ConfigureSecureApp();

    // ===================================================================
    // INICIALIZAÇÃO E VERIFICAÇÕES
    // ===================================================================

    // Log de inicialização
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("=== ProGestao Iniciando - Arquitetura Refatorada ===");

    // Verificação de services registrados (apenas em Development)
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Verificar se todos os services principais estão registrados
            var queryService = services.GetRequiredService<IAtividadeQueryService>();
            var commandService = services.GetRequiredService<IAtividadeCommandService>();
            var validationService = services.GetRequiredService<IAtividadeValidationService>();
            var lookupService = services.GetRequiredService<ILookupService>();

            logger.LogInformation("✅ Todos os services foram registrados com sucesso");
            logger.LogInformation("✅ Query Service: {Type}", queryService.GetType().Name);
            logger.LogInformation("✅ Command Service: {Type}", commandService.GetType().Name);
            logger.LogInformation("✅ Validation Service: {Type}", validationService.GetType().Name);
            logger.LogInformation("✅ Lookup Service: {Type}", lookupService.GetType().Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro na verificação de services");
        }
    }

    app.Run();


  //  // Log de inicialização 
  //  var appSettings = app.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
  //  app.Logger.LogInformation($"🚀 {appSettings?.AppName} v{appSettings?.Version} iniciado em ambiente {appSettings?.Environment}");
  //
  //  // Pipeline configuration 
  //  app.UseHttpsRedirection();
  //  app.UseRouting();
  //  app.UseAuthorization();
  //
  //  app.MapStaticAssets();
  //  app.MapRazorPages()
  //     .WithStaticAssets();
  //
  //  app.Run();
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Erros de configuração"))
{
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ ERRO DE CONFIGURAÇÃO:");
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    Console.WriteLine("📋 SOLUÇÕES:");
    Console.WriteLine("1. Configure User Secrets: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"sua_connection_string\"");
    Console.WriteLine("2. Configure variável de ambiente: ConnectionStrings__DefaultConnection");
    Console.WriteLine("3. Crie arquivo appsettings.Development.json com dados reais");
    Console.ResetColor();
    Environment.Exit(1);
}
catch (Exception ex)
{
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ ERRO CRÍTICO NA INICIALIZAÇÃO: {ex.Message}");

    if (ex.Message.Contains("DbContext") || ex.Message.Contains("service"))
    {
        Console.WriteLine();
        Console.WriteLine("🔍 POSSÍVEIS CAUSAS:");
        Console.WriteLine("1. Configuração duplicada do DbContext");
        Console.WriteLine("2. Conflito entre AddDbContext e AddDbContextPool");
        Console.WriteLine("3. Serviços registrados incorretamente");
        Console.WriteLine();
        Console.WriteLine("💡 SOLUÇÕES:");
        Console.WriteLine("1. Verifique se não há múltiplas configurações do DbContext");
        Console.WriteLine("2. Certifique-se que usa apenas AddDbContextPool");
        Console.WriteLine("3. Remova configurações antigas do Entity Framework");
    }
    Console.ResetColor();
    Environment.Exit(1);
}

/// <summary>
/// Extensões para configuração adicional
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona configurações específicas de performance se necessário
    /// </summary>
    public static IServiceCollection AddPerformanceOptimizations(this IServiceCollection services)
    {
        // Cache em memória para otimizações futuras
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Limite de cache
        });

        // Compression para responses grandes
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return services;
    }
}