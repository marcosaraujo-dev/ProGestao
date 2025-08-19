using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Pages.Projetos
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
        public Projeto? Projeto { get; set; }

        // Propriedades para estatísticas de atividades relacionadas
        public int AtividadesRelacionadas { get; set; }
        public int AtividadesEmAndamento { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesPendentes { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando projeto {ProjetoId} para exclusão", id);

                var projeto = await _context.Projetos
                    .Include(p => p.Responsavel)
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto com ID {ProjetoId} não encontrado para exclusão", id);
                    TempData["ErrorMessage"] = $"Projeto com ID {id} não encontrado.";
                    return RedirectToPage("./Index");
                }

                // Carregar estatísticas de atividades relacionadas
                await CarregarEstatisticasAtividades(id);

                // Log das entidades relacionadas para debug
                _logger.LogDebug("Projeto carregado: {ProjetoNome}, Responsavel: {ResponsavelNome}, Status: {StatusNome}, Atividades: {AtividadesCount}",
                    projeto.Nome,
                    projeto.Responsavel?.Nome ?? "NULL",
                    projeto.Status?.Nome ?? "NULL",
                    AtividadesRelacionadas);

                // Verificar se há entidades relacionadas nulas
                var erros = new List<string>();

                if (projeto.Responsavel == null)
                {
                    _logger.LogWarning("Responsável não encontrado para projeto {ProjetoId}, ResponsavelId: {ResponsavelId}",
                        id, projeto.ResponsavelId);
                    erros.Add("Responsável não encontrado");
                }

                if (projeto.Status == null)
                {
                    _logger.LogWarning("Status não encontrado para projeto {ProjetoId}, StatusId: {StatusId}",
                        id, projeto.StatusId);
                    erros.Add("Status não encontrado");
                }

                // Se há dados inválidos, ainda assim mostrar a tela mas com warning
                if (erros.Any())
                {
                    TempData["WarningMessage"] = $"Alguns dados relacionados não foram encontrados: {string.Join(", ", erros)}. " +
                                                "A exclusão ainda é possível, mas verifique se há inconsistências no banco de dados.";
                }

                // Warning especial se há atividades relacionadas
                if (AtividadesRelacionadas > 0)
                {
                    TempData["WarningMessage"] = $"Este projeto possui {AtividadesRelacionadas} atividade(s) relacionada(s) que também serão excluídas!";
                }

                Projeto = projeto;
                _logger.LogInformation("Projeto {ProjetoId} carregado com sucesso para confirmação de exclusão", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar projeto {ProjetoId} para exclusão", id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão do projeto {ProjetoId}", id);

                var projeto = await _context.Projetos
                    .Include(p => p.Responsavel)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Tentativa de exclusão de projeto inexistente: ID {ProjetoId}", id);
                    TempData["ErrorMessage"] = "Projeto não encontrado.";
                    return RedirectToPage("./Index");
                }

                // Verificar se o projeto pode ser excluído
                var validationResult = await ValidarExclusao(projeto);
                if (!validationResult.CanDelete)
                {
                    _logger.LogWarning("Exclusão do projeto {ProjetoId} bloqueada: {Motivo}", id, validationResult.ErrorMessage);
                    TempData["ErrorMessage"] = validationResult.ErrorMessage;
                    return RedirectToPage("./Index");
                }

                var nomeProjeto = projeto.Nome;
                var nomeResponsavel = projeto.Responsavel?.Nome ?? "Responsável não encontrado";

                // Contar atividades antes da exclusão para log
                var atividadesCount = await _context.Atividades.CountAsync(a => a.ProjetoId == id);

                // Iniciar transação para garantir consistência
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Excluir todas as atividades relacionadas primeiro
                    var atividades = await _context.Atividades
                        .Where(a => a.ProjetoId == id)
                        .ToListAsync();

                    if (atividades.Any())
                    {
                        _context.Atividades.RemoveRange(atividades);
                        _logger.LogInformation("Removendo {AtividadesCount} atividades do projeto {ProjetoId}",
                            atividades.Count, id);
                    }

                    // Excluir o projeto
                    _context.Projetos.Remove(projeto);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Projeto '{ProjetoNome}' (ID: {ProjetoId}) do responsável '{ResponsavelNome}' foi excluído com sucesso junto com {AtividadesCount} atividade(s)",
                        nomeProjeto, id, nomeResponsavel, atividadesCount);

                    TempData["SuccessMessage"] = $"Projeto '{nomeProjeto}' excluído com sucesso! " +
                                               (atividadesCount > 0 ? $"({atividadesCount} atividade(s) também foram removidas)" : "");

                    return RedirectToPage("./Index");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao excluir projeto {ProjetoId}", id);

                // Verificar se é violação de constraint
                if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
                    ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                {
                    TempData["ErrorMessage"] = "Não foi possível excluir o projeto. Ele possui dependências que impedem sua exclusão. " +
                                              "Verifique se há registros relacionados (logs, relatórios, etc.) e remova-os primeiro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro de banco de dados ao excluir o projeto. Tente novamente.";
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir projeto {ProjetoId}", id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        private async Task<(bool CanDelete, string ErrorMessage)> ValidarExclusao(Projeto projeto)
        {
            try
            {
                // Verificar se o projeto está em andamento
                if (projeto.StatusId != 0) // Assumindo que existe um status
                {
                    var status = await _context.StatusProjetos
                        .FirstOrDefaultAsync(s => s.Id == projeto.StatusId);

                    if (status?.Nome == "Em Andamento")
                    {
                        return (false, "Não é possível excluir projetos em andamento. Altere o status primeiro.");
                    }
                }

                // Verificar se há atividades em andamento
                var atividadesEmAndamento = await _context.Atividades
                    .Include(a => a.Status)
                    .Where(a => a.ProjetoId == projeto.Id)
                    .CountAsync(a => a.Status != null && a.Status.Nome == "Em Andamento");

                if (atividadesEmAndamento > 0)
                {
                    return (false, $"Este projeto possui {atividadesEmAndamento} atividade(s) em andamento. " +
                                  "Finalize ou cancele essas atividades antes de excluir o projeto.");
                }

                // Verificar se é um projeto crítico (pode ser implementado conforme regras de negócio)
                var temAtividadesCriticas = await _context.Atividades
                    .Where(a => a.ProjetoId == projeto.Id && a.Prioridade == 4) // Crítica
                    .AnyAsync();

                if (temAtividadesCriticas)
                {
                    _logger.LogInformation("Projeto {ProjetoId} tem atividades críticas, mas exclusão será permitida após confirmação",
                        projeto.Id);
                }

                // Verificar se o projeto tem data de fim prevista muito próxima (warning, não bloqueio)
                if (projeto.DataFimPrevista.HasValue &&
                    projeto.DataFimPrevista.Value > DateTime.Now &&
                    projeto.DataFimPrevista.Value <= DateTime.Now.AddDays(7))
                {
                    _logger.LogInformation("Projeto {ProjetoId} tem data de fim prevista para os próximos 7 dias",
                        projeto.Id);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação de exclusão do projeto {ProjetoId}", projeto.Id);
                return (false, "Erro durante validação. Tente novamente.");
            }
        }

        /// <summary>
        /// Carrega estatísticas das atividades relacionadas ao projeto
        /// </summary>
        /// <param name="projetoId">ID do projeto</param>
        private async Task CarregarEstatisticasAtividades(int projetoId)
        {
            try
            {
                AtividadesRelacionadas = await _context.Atividades
                    .CountAsync(a => a.ProjetoId == projetoId);

                if (AtividadesRelacionadas > 0)
                {
                    var atividades = await _context.Atividades
                        .Include(a => a.Status)
                        .Where(a => a.ProjetoId == projetoId)
                        .ToListAsync();

                    AtividadesEmAndamento = atividades.Count(a => a.Status?.Nome == "Em Andamento");
                    AtividadesConcluidas = atividades.Count(a => a.Status?.Nome == "Concluída");
                    AtividadesPendentes = atividades.Count(a => a.Status?.Nome == "Pendente" || a.Status == null);
                }

                _logger.LogDebug("Estatísticas carregadas para projeto {ProjetoId}: Total={Total}, EmAndamento={EmAndamento}, Concluidas={Concluidas}, Pendentes={Pendentes}",
                    projetoId, AtividadesRelacionadas, AtividadesEmAndamento, AtividadesConcluidas, AtividadesPendentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar estatísticas de atividades para projeto {ProjetoId}", projetoId);
                // Em caso de erro, manter valores zerados
                AtividadesRelacionadas = 0;
                AtividadesEmAndamento = 0;
                AtividadesConcluidas = 0;
                AtividadesPendentes = 0;
            }
        }

        /// <summary>
        /// Método auxiliar para verificar se um projeto existe
        /// </summary>
        /// <param name="id">ID do projeto</param>
        /// <returns>True se existe, False caso contrário</returns>
        private async Task<bool> ProjetoExistsAsync(int id)
        {
            try
            {
                return await _context.Projetos.AnyAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do projeto {ProjetoId}", id);
                return false;
            }
        }

        /// <summary>
        /// Método para obter resumo de impacto da exclusão
        /// </summary>
        /// <param name="projetoId">ID do projeto</param>
        /// <returns>Resumo dos impactos</returns>
        public async Task<string> ObterResumoImpactoExclusao(int projetoId)
        {
            try
            {
                await CarregarEstatisticasAtividades(projetoId);

                var impactos = new List<string>();

                if (AtividadesRelacionadas > 0)
                {
                    impactos.Add($"{AtividadesRelacionadas} atividade(s) será(ão) excluída(s)");
                }

                if (AtividadesEmAndamento > 0)
                {
                    impactos.Add($"{AtividadesEmAndamento} atividade(s) em andamento será(ão) perdida(s)");
                }

                return impactos.Any() ? string.Join(", ", impactos) : "Nenhum impacto adicional identificado";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resumo de impacto para projeto {ProjetoId}", projetoId);
                return "Erro ao calcular impacto";
            }
        }
    }
}