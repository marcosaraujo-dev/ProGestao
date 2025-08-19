using System.Data.SqlClient;

namespace ProGestao.Configuration
{
    public static class ConfigurationValidator
    {
        public static void ValidateConfiguration(IConfiguration configuration)
        {
            var errors = new List<string>();

            // Validar Connection String
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                errors.Add("Connection String 'DefaultConnection' não configurada");
            }
            else if (connectionString.Contains("CONFIGURE_VIA_USER_SECRETS") ||
                     connectionString.Contains("DEFINIR_VIA_VARIAVEIS_AMBIENTE"))
            {
                errors.Add("Connection String ainda está com valor de template. Configure via User Secrets ou Variáveis de Ambiente");
            }
            else
            {
                // Tentar validar a connection string
                try
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    if (string.IsNullOrEmpty(builder.DataSource))
                    {
                        errors.Add("Server não especificado na Connection String");
                    }
                    if (string.IsNullOrEmpty(builder.InitialCatalog))
                    {
                        errors.Add("Database não especificado na Connection String");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Connection String inválida: {ex.Message}");
                }
            }

            // Validar outras configurações críticas
            var appSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
            if (appSettings == null)
            {
                errors.Add("Seção 'ApplicationSettings' não encontrada");
            }

            if (errors.Any())
            {
                var errorMessage = "Erros de configuração encontrados:\n" + string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
                throw new InvalidOperationException(errorMessage);
            }
        }

        public static async Task<bool> TestDatabaseConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
