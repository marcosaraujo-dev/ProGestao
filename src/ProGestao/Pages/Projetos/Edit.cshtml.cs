using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Pages.Projetos
{
    public class EditModel : PageModel
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ProGestaoContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Projeto Projeto { get; set; } = default!;

        // SelectLists para dropdowns
        public SelectList StatusSelectList { get; set; } = default!;
        public SelectList ResponsaveisSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando projeto {ProjetoId} para edição", id);

                var projeto = await _context.Projetos
                    .Include(p => p.Responsavel)
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto com ID {ProjetoId} não encontrado para edição", id);
                    TempData["ErrorMessage"] = $"Projeto com ID {id} não encontrado.";
                    return RedirectToPage("./Index");
                }

                Projeto = projeto;
                await CarregarSelectLists();

                _logger.LogInformation("Projeto {ProjetoId} carregado com sucesso para edição", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar projeto {ProjetoId} para edição", id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido ao tentar salvar projeto {ProjetoId}", Projeto.Id);
                    await CarregarSelectLists();
                    return Page();
                }

                _logger.LogInformation("Iniciando atualização do projeto {ProjetoId}", Projeto.Id);

                // Verificar se o projeto existe
                var projetoExistente = await _context.Projetos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == Projeto.Id);

                if (projetoExistente == null)
                {
                    _logger.LogWarning("Tentativa de atualizar projeto inexistente: ID {ProjetoId}", Projeto.Id);
                    TempData["ErrorMessage"] = "Projeto não encontrado.";
                    return RedirectToPage("./Index");
                }

                // Validações de negócio
                var validationResult = await ValidarEdicao(Projeto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validação falhou para projeto {ProjetoId}: {Erro}", Projeto.Id, validationResult.ErrorMessage);
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    await CarregarSelectLists();
                    return Page();
                }

                // Preservar dados que não devem ser alterados
                Projeto.DataCriacao = projetoExistente.DataCriacao;

                // Atualizar a entidade no contexto
                _context.Entry(Projeto).State = EntityState.Modified;
                _context.Entry(Projeto).Property(p => p.DataCriacao).IsModified = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Projeto '{ProjetoNome}' (ID: {ProjetoId}) atualizado com sucesso",
                    Projeto.Nome, Projeto.Id);

                TempData["SuccessMessage"] = $"Projeto '{Projeto.Nome}' atualizado com sucesso!";
                return RedirectToPage("./Details", new { id = Projeto.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao atualizar projeto {ProjetoId}", Projeto.Id);

                if (!await ProjetoExists(Projeto.Id))
                {
                    TempData["ErrorMessage"] = "Projeto não encontrado. Pode ter sido excluído por outro usuário.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "O projeto foi modificado por outro usuário. Recarregue a página e tente novamente.";
                    return RedirectToPage("./Edit", new { id = Projeto.Id });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao atualizar projeto {ProjetoId}", Projeto.Id);
                ModelState.AddModelError(string.Empty, "Erro ao salvar as alterações. Verifique os dados e tente novamente.");
                await CarregarSelectLists();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar projeto {ProjetoId}", Projeto.Id);
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
                await CarregarSelectLists();
                return Page();
            }
        }

        /// <summary>
        /// Valida as regras de negócio para edição do projeto
        /// </summary>
        /// <param name="projeto">Projeto a ser validado</param>
        /// <returns>Resultado da validação</returns>
        private async Task<(bool IsValid, string ErrorMessage)> ValidarEdicao(Projeto projeto)
        {
            try
            {
                // Validar se as datas são consistentes
                if (projeto.DataFimPrevista.HasValue && projeto.DataFimPrevista.Value < projeto.DataInicio)
                {
                    return (false, "A data de fim prevista não pode ser anterior à data de início.");
                }

                // Validar se o responsável existe
               // if (projeto.ResponsavelId != 0)
               // {
               //     var responsavelExists = await _context.Usuarios.AnyAsync(u => u.Id == projeto.ResponsavelId);
               //     if (!responsavelExists)
               //     {
               //         return (false, "Responsável selecionado não encontrado.");
               //     }
               // }

                // Validar se o status existe
                if (projeto.StatusId != 0)
                {
                    var statusExists = await _context.StatusProjetos.AnyAsync(s => s.Id == projeto.StatusId);
                    if (!statusExists)
                    {
                        return (false, "Status selecionado não encontrado.");
                    }
                }

                // Validar se já existe um projeto com o mesmo nome (exceto o atual)
                var nomeJaExiste = await _context.Projetos
                    .AnyAsync(p => p.Nome.ToLower() == projeto.Nome.ToLower() && p.Id != projeto.Id);

                if (nomeJaExiste)
                {
                    return (false, "Já existe um projeto com este nome.");
                }

                // Validar URL se fornecida
                if (!string.IsNullOrEmpty(projeto.LinkProjeto))
                {
                    if (!Uri.TryCreate(projeto.LinkProjeto, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        return (false, "Link do projeto deve ser uma URL válida (http:// ou https://).");
                    }
                }

                // Validar se as mudanças de status são permitidas
                var projetoOriginal = await _context.Projetos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projeto.Id);

                if (projetoOriginal != null && projetoOriginal.StatusId != projeto.StatusId)
                {
                    var mudancaPermitida = await ValidarMudancaStatus(projetoOriginal.StatusId, projeto.StatusId);
                    if (!mudancaPermitida.IsValid)
                    {
                        return (false, mudancaPermitida.ErrorMessage);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação de edição do projeto {ProjetoId}", projeto.Id);
                return (false, "Erro durante validação. Tente novamente.");
            }
        }

        /// <summary>
        /// Valida se a mudança de status é permitida
        /// </summary>
        /// <param name="statusAtualId">ID do status atual</param>
        /// <param name="novoStatusId">ID do novo status</param>
        /// <returns>Resultado da validação</returns>
        private async Task<(bool IsValid, string ErrorMessage)> ValidarMudancaStatus(int statusAtualId, int novoStatusId)
        {
            try
            {
                var statusAtual = await _context.StatusProjetos.FindAsync(statusAtualId);
                var novoStatus = await _context.StatusProjetos.FindAsync(novoStatusId);

                if (statusAtual == null || novoStatus == null)
                {
                    return (true, string.Empty); // Se não encontrar os status, permite a mudança
                }

              
                if (statusAtual.Nome == "Concluído" && novoStatus.Nome != "Concluído")
                {
                    return (false, "Não é possível alterar o status de um projeto já concluído.");
                }

                if (statusAtual.Nome == "Cancelado" && novoStatus.Nome == "Em Andamento")
                {
                    return (false, "Não é possível reativar um projeto cancelado diretamente. Altere primeiro para 'Planejamento'.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar mudança de status de {StatusAtual} para {NovoStatus}",
                    statusAtualId, novoStatusId);
                return (false, "Erro ao validar mudança de status.");
            }
        }

        /// <summary>
        /// Carrega as listas de seleção para os dropdowns
        /// </summary>
        private async Task CarregarSelectLists()
        {
            try
            {
                // Carregar status de projetos
                var statusList = await _context.StatusProjetos
                    .OrderBy(s => s.Nome)
                    .Select(s => new { s.Id, s.Nome })
                    .ToListAsync();

                StatusSelectList = new SelectList(statusList, "Id", "Nome", Projeto?.StatusId);

                // Carregar usuários (responsáveis)
                var responsaveisList = await _context.Usuarios
                    .Where(u => u.Ativo) // Assumindo que existe um campo Ativo
                    .OrderBy(u => u.Nome)
                    .Select(u => new { u.Id, u.Nome })
                    .ToListAsync();

                ResponsaveisSelectList = new SelectList(responsaveisList, "Id", "Nome", Projeto?.ResponsavelId);

                _logger.LogDebug("SelectLists carregadas: {StatusCount} status, {ResponsaveisCount} responsáveis",
                    statusList.Count, responsaveisList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar SelectLists");

                // Criar listas vazias em caso de erro
                StatusSelectList = new SelectList(new List<object>(), "Id", "Nome");
                ResponsaveisSelectList = new SelectList(new List<object>(), "Id", "Nome");

                throw;
            }
        }

        /// <summary>
        /// Verifica se um projeto existe
        /// </summary>
        /// <param name="id">ID do projeto</param>
        /// <returns>True se existe, False caso contrário</returns>
        private async Task<bool> ProjetoExists(int id)
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
        /// Método auxiliar para obter dados adicionais do projeto
        /// </summary>
        /// <param name="projetoId">ID do projeto</param>
        /// <returns>Dados adicionais</returns>
        public async Task<object> ObterDadosAdicionais(int projetoId)
        {
            try
            {
                var atividadesCount = await _context.Atividades.CountAsync(a => a.ProjetoId == projetoId);
                var atividadesConcluidas = await _context.Atividades
                    .Include(a => a.Status)
                    .CountAsync(a => a.ProjetoId == projetoId && a.Status != null && a.Status.Nome == "Concluída");

                var percentualConclusao = atividadesCount > 0 ?
                    Math.Round((double)atividadesConcluidas / atividadesCount * 100, 1) : 0;

                return new
                {
                    TotalAtividades = atividadesCount,
                    AtividadesConcluidas = atividadesConcluidas,
                    PercentualConclusao = percentualConclusao
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados adicionais do projeto {ProjetoId}", projetoId);
                return new { TotalAtividades = 0, AtividadesConcluidas = 0, PercentualConclusao = 0 };
            }
        }
    }
}