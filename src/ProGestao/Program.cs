using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ProGestao.Configuration;
using ProGestao.Data;
using ProGestao.Services;


var builder = WebApplication.CreateBuilder(args);

try
{
    // Configurar fontes de configuração
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>(optional: true);

    // Adicionar serviços com configuração segura
    builder.Services.AddRazorPages();
    builder.Services.AddSecureConfiguration(builder.Configuration);

    //Injeção
    builder.Services.AddScoped<ITimelineService, TimelineService>();

    builder.Services.AddScoped<IProjetoService, ProjetoService>();
    builder.Services.AddScoped<IAtividadeService, AtividadeService>();
    builder.Services.AddScoped<IProjetoService, ProjetoService>();
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<IGridService, GridService>();

    // Logging
    builder.Services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        if (builder.Environment.IsDevelopment())
        {
            logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
        }
    });

    builder.Services.AddDbContext<ProGestaoContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    var app = builder.Build();

    // Testar conexão com banco na inicialização (apenas em desenvolvimento)
    if (app.Environment.IsDevelopment())
    {
        var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
        var canConnect = await ConfigurationValidator.TestDatabaseConnectionAsync(connectionString!);

        if (!canConnect)
        {
            app.Logger.LogWarning("⚠️  Não foi possível conectar ao banco de dados. Verifique a connection string.");
        }
        else
        {
            app.Logger.LogInformation("✅ Conexão com banco de dados validada com sucesso!");
        }
    }


    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    // Configurar pipeline com segurança
    app.ConfigureSecureApp();
    app.MapRazorPages();

    // Log de inicialização
    var appSettings = app.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
    app.Logger.LogInformation($"🚀 {appSettings?.AppName} v{appSettings?.Version} iniciado em ambiente {appSettings?.Environment}");


    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapRazorPages()
       .WithStaticAssets();


    app.Run();
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
    Console.ResetColor();
    Environment.Exit(1);
}