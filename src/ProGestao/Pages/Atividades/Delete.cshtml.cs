using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Pages.Atividades
{
    public class DeleteModel : PageModel
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ProGestaoContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Atividade? Atividade { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando atividade {AtividadeId} para exclusão", id);

                var atividade = await _context.Atividades
                    .Include(a => a.Projeto)
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Status)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atividade == null)
                {
                    _logger.LogWarning("Atividade com ID {AtividadeId} não encontrada para exclusão", id);
                    TempData["ErrorMessage"] = $"Atividade com ID {id} não encontrada.";
                    return RedirectToPage("./Index");
                }

                // Log das entidades relacionadas para debug
                _logger.LogDebug("Atividade carregada: {AtividadeNome}, Projeto: {ProjetoNome}, Usuario: {UsuarioNome}, Tipo: {TipoNome}, Status: {StatusNome}",
                    atividade.Nome,
                    atividade.Projeto?.Nome ?? "NULL",
                    atividade.Usuario?.Nome ?? "NULL",
                    atividade.TipoAtividade?.Nome ?? "NULL",
                    atividade.Status?.Nome ?? "NULL");

                // Verificar se há entidades relacionadas nulas
                var erros = new List<string>();

                if (atividade.Usuario == null)
                {
                    _logger.LogWarning("Usuário não encontrado para atividade {AtividadeId}, UsuarioId: {UsuarioId}",
                        id, atividade.UsuarioId);
                    erros.Add("Usuário responsável não encontrado");
                }

                if (atividade.TipoAtividade == null)
                {
                    _logger.LogWarning("Tipo de atividade não encontrado para atividade {AtividadeId}, TipoAtividadeId: {TipoId}",
                        id, atividade.TipoAtividadeId);
                    erros.Add("Tipo de atividade não encontrado");
                }

                if (atividade.Status == null)
                {
                    _logger.LogWarning("Status não encontrado para atividade {AtividadeId}, StatusId: {StatusId}",
                        id, atividade.StatusId);
                    erros.Add("Status não encontrado");
                }

                // Se há dados inválidos, ainda assim mostrar a tela mas com warning
                if (erros.Any())
                {
                    TempData["WarningMessage"] = $"Alguns dados relacionados não foram encontrados: {string.Join(", ", erros)}. " +
                                                "A exclusão ainda é possível, mas verifique se há inconsistências no banco de dados.";
                }

                Atividade = atividade;
                _logger.LogInformation("Atividade {AtividadeId} carregada com sucesso para confirmação de exclusão", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade {AtividadeId} para exclusão", id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão da atividade {AtividadeId}", id);

                var atividade = await _context.Atividades
                    .Include(a => a.Projeto)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atividade == null)
                {
                    _logger.LogWarning("Tentativa de exclusão de atividade inexistente: ID {AtividadeId}", id);
                    TempData["ErrorMessage"] = "Atividade não encontrada.";
                    return RedirectToPage("./Index");
                }

                // Verificar se a atividade pode ser excluída
                var validationResult = await ValidarExclusao(atividade);
                if (!validationResult.CanDelete)
                {
                    _logger.LogWarning("Exclusão da atividade {AtividadeId} bloqueada: {Motivo}", id, validationResult.ErrorMessage);
                    TempData["ErrorMessage"] = validationResult.ErrorMessage;
                    return RedirectToPage("./Index");
                }

                var nomeAtividade = atividade.Nome;
                var nomeProjeto = atividade.Projeto?.Nome ?? "Sem projeto";

                // Excluir a atividade
                _context.Atividades.Remove(atividade);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Atividade '{AtividadeNome}' (ID: {AtividadeId}) do projeto '{ProjetoNome}' foi excluída com sucesso",
                    nomeAtividade, id, nomeProjeto);

                TempData["SuccessMessage"] = $"Atividade '{nomeAtividade}' excluída com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao excluir atividade {AtividadeId}", id);

                // Verificar se é violação de constraint
                if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
                    ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                {
                    TempData["ErrorMessage"] = "Não foi possível excluir a atividade. Ela possui dependências que impedem sua exclusão. " +
                                              "Verifique se há registros relacionados (logs, comentários, etc.) e remova-os primeiro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro de banco de dados ao excluir a atividade. Tente novamente.";
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir atividade {AtividadeId}", id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        private async Task<(bool CanDelete, string ErrorMessage)> ValidarExclusao(Atividade atividade)
        {
            try
            {
                
                // Verificar se a atividade está em andamento
                if (atividade.StatusId != 0) // Assumindo que existe um status
                {
                    var status = await _context.StatusAtividades
                        .FirstOrDefaultAsync(s => s.Id == atividade.StatusId);

                    if (status?.Nome == "Em Andamento")
                    {
                        return (false, "Não é possível excluir atividades em andamento. Altere o status primeiro.");
                    }
                }

                // Verificar se a atividade é crítica para o projeto
                if (atividade.Prioridade == 4) // Crítica
                {
                    var atividadesCriticasProjeto = await _context.Atividades
                        .Where(a => a.ProjetoId == atividade.ProjetoId && a.Prioridade == 4 && a.Id != atividade.Id)
                        .CountAsync();

                    if (atividadesCriticasProjeto == 0 && atividade.ProjetoId.HasValue)
                    {
                        return (false, "Esta é a única atividade crítica do projeto. " +
                                      "Considere alterar a prioridade ou adicionar outras atividades críticas antes de excluir.");
                    }
                }

                // Verificar se tem horas registradas
                if (atividade.HorasReais.HasValue && atividade.HorasReais.Value > 0)
                {
                    _logger.LogInformation("Atividade {AtividadeId} tem {Horas} horas registradas, mas exclusão será permitida",
                        atividade.Id, atividade.HorasReais.Value);
                   
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação de exclusão da atividade {AtividadeId}", atividade.Id);
                return (false, "Erro durante validação. Tente novamente.");
            }
        }

        /// <summary>
        /// Método auxiliar para verificar se uma atividade existe
        /// </summary>
        /// <param name="id">ID da atividade</param>
        /// <returns>True se existe, False caso contrário</returns>
        private async Task<bool> AtividadeExistsAsync(int id)
        {
            try
            {
                return await _context.Atividades.AnyAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência da atividade {AtividadeId}", id);
                return false;
            }
        }

        /// <summary>
        /// Método para carregar dados adicionais se necessário
        /// </summary>
        private async Task CarregarDadosAdicionais()
        {
            try
            {
               
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados adicionais");
                throw;
            }
        }
    }
}