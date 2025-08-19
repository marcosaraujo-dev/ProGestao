using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProGestao.Pages.Shared
{
    /// <summary>
    /// Classe base para todos os PageModels com funcionalidades comuns de TempData
    /// </summary>
    public abstract class BasePageModel : PageModel
    {
        /// <summary>
        /// Limpa mensagens TempData antigas para evitar que fiquem aparecendo indefinidamente
        /// </summary>
        protected void LimparTempDataAntigo()
        {
            // Remove mensagens que possam ter ficado "grudadas"
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("WarningMessage");
            TempData.Remove("InfoMessage");
        }

        /// <summary>
        /// Define uma mensagem de sucesso temporária
        /// </summary>
        /// <param name="mensagem">Mensagem a ser exibida</param>
        protected void DefinirMensagemSucesso(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["SuccessMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define uma mensagem de erro temporária
        /// </summary>
        /// <param name="mensagem">Mensagem a ser exibida</param>
        protected void DefinirMensagemErro(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["ErrorMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define uma mensagem de aviso temporária
        /// </summary>
        /// <param name="mensagem">Mensagem a ser exibida</param>
        protected void DefinirMensagemAviso(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["WarningMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define uma mensagem informativa temporária
        /// </summary>
        /// <param name="mensagem">Mensagem a ser exibida</param>
        protected void DefinirMensagemInfo(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["InfoMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Verifica se existe alguma mensagem de erro no TempData
        /// </summary>
        /// <returns>True se há mensagens de erro</returns>
        protected bool TemMensagemErro()
        {
            return TempData.ContainsKey("ErrorMessage") &&
                   !string.IsNullOrWhiteSpace(TempData["ErrorMessage"]?.ToString());
        }

        /// <summary>
        /// Verifica se existe alguma mensagem de sucesso no TempData
        /// </summary>
        /// <returns>True se há mensagens de sucesso</returns>
        protected bool TemMensagemSucesso()
        {
            return TempData.ContainsKey("SuccessMessage") &&
                   !string.IsNullOrWhiteSpace(TempData["SuccessMessage"]?.ToString());
        }

        /// <summary>
        /// Define uma mensagem baseada no resultado de uma operação
        /// </summary>
        /// <param name="sucesso">Se a operação foi bem-sucedida</param>
        /// <param name="mensagemSucesso">Mensagem para caso de sucesso</param>
        /// <param name="mensagemErro">Mensagem para caso de erro</param>
        protected void DefinirMensagemPorResultado(bool sucesso, string mensagemSucesso, string mensagemErro)
        {
            if (sucesso)
            {
                DefinirMensagemSucesso(mensagemSucesso);
            }
            else
            {
                DefinirMensagemErro(mensagemErro);
            }
        }

        /// <summary>
        /// Define mensagem de sucesso para operações CRUD padrão
        /// </summary>
        /// <param name="operacao">Tipo da operação (criar, atualizar, excluir)</param>
        /// <param name="entidade">Nome da entidade</param>
        /// <param name="nome">Nome específico do item (opcional)</param>
        protected void DefinirMensagemSucessoCrud(string operacao, string entidade, string nome = null)
        {
            var nomeItem = !string.IsNullOrWhiteSpace(nome) ? $" '{nome}'" : "";
            var mensagem = operacao.ToLower() switch
            {
                "criar" or "criado" or "criada" => $"{entidade}{nomeItem} criado com sucesso!",
                "atualizar" or "atualizado" or "atualizada" or "editar" => $"{entidade}{nomeItem} atualizado com sucesso!",
                "excluir" or "excluído" or "excluída" or "deletar" => $"{entidade}{nomeItem} excluído com sucesso!",
                _ => $"{entidade}{nomeItem} processado com sucesso!"
            };

            DefinirMensagemSucesso(mensagem);
        }

        /// <summary>
        /// Define mensagem de erro para operações CRUD padrão
        /// </summary>
        /// <param name="operacao">Tipo da operação (criar, atualizar, excluir)</param>
        /// <param name="entidade">Nome da entidade</param>
        protected void DefinirMensagemErroCrud(string operacao, string entidade)
        {
            var mensagem = operacao.ToLower() switch
            {
                "criar" or "criado" or "criada" => $"Erro ao criar {entidade.ToLower()}. Tente novamente.",
                "atualizar" or "atualizado" or "atualizada" or "editar" => $"Erro ao atualizar {entidade.ToLower()}. Tente novamente.",
                "excluir" or "excluído" or "excluída" or "deletar" => $"Erro ao excluir {entidade.ToLower()}. Tente novamente.",
                "carregar" => $"Erro ao carregar {entidade.ToLower()}. Tente novamente.",
                _ => $"Erro ao processar {entidade.ToLower()}. Tente novamente."
            };

            DefinirMensagemErro(mensagem);
        }

        /// <summary>
        /// Método para ser chamado no início de OnGetAsync para limpeza
        /// </summary>
        protected virtual void InicializarPagina()
        {
            LimparTempDataAntigo();
        }

        /// <summary>
        /// Helper para logging com contexto da página
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="message">Mensagem de log</param>
        /// <param name="level">Nível do log</param>
        protected void LogComContexto(ILogger logger, string message, LogLevel level = LogLevel.Information)
        {
            var paginaAtual = GetType().Name.Replace("Model", "");
            var mensagemCompleta = $"[{paginaAtual}] {message}";

            switch (level)
            {
                case LogLevel.Error:
                    logger.LogError(mensagemCompleta);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(mensagemCompleta);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(mensagemCompleta);
                    break;
                default:
                    logger.LogInformation(mensagemCompleta);
                    break;
            }
        }
    }
}