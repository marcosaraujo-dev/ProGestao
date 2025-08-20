using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Pages.Projetos
{
    /// <summary>
    /// PageModel refatorado para exclusão de projetos
    /// </summary>
    public class DeleteModel : PageModel
    {
        #region Dependencies

        private readonly IProjetoQueryService _queryService;
        private readonly IProjetoCommandService _commandService;
        private readonly ILogger<DeleteModel> _logger;

        #endregion

        #region Constructor

        public DeleteModel(
            IProjetoQueryService queryService,
            IProjetoCommandService commandService,
            ILogger<DeleteModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public ProjetoViewModel? Projeto { get; set; }
        public int AtividadesRelacionadas { get; set; }

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando projeto {ProjetoId} para exclusão", id);

                Projeto = await _queryService.GetProjetoComAtividadesAsync(id);
                if (Projeto == null)
                {
                    _logger.LogWarning("Projeto {ProjetoId} não encontrado para exclusão", id);
                    TempData["ErrorMessage"] = "Projeto não encontrado.";
                    return RedirectToPage("./Index");
                }

                AtividadesRelacionadas = await _queryService.GetQuantidadeAtividadesAsync(id);

                _logger.LogInformation("Projeto {ProjetoId} carregado para exclusão. Atividades relacionadas: {Count}",
                    id, AtividadesRelacionadas);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar projeto {ProjetoId} para exclusão", id);
                TempData["ErrorMessage"] = "Erro ao carregar projeto. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão do projeto {ProjetoId}", id);

                var result = await _commandService.DeleteProjetoAsync(id);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Projeto {ProjetoId} excluído com sucesso", id);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na exclusão do projeto {ProjetoId}: {Message}", id, result.Message);

                    TempData["ErrorMessage"] = result.Errors.Any()
                        ? string.Join("; ", result.Errors)
                        : result.Message;

                    return RedirectToPage("./Delete", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir projeto {ProjetoId}", id);
                TempData["ErrorMessage"] = "Erro interno ao excluir projeto. Tente novamente.";
                return RedirectToPage("./Delete", new { id });
            }
        }

        #endregion
    }
}