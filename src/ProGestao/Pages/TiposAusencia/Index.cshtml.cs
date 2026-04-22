using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Pages.TiposAusencia
{
    /// <summary>
    /// PageModel para listagem de tipos de ausência
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Dependencies

        private readonly ITipoAusenciaQueryService _queryService;
        private readonly ITipoAusenciaCommandService _commandService;
        private readonly ILogger<IndexModel> _logger;

        #endregion

        #region Constructor

        public IndexModel(
            ITipoAusenciaQueryService queryService,
            ITipoAusenciaCommandService commandService,
            ILogger<IndexModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public IList<TipoAusenciaViewModel> TiposAusencia { get; set; } = new List<TipoAusenciaViewModel>();

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando lista de tipos de ausência");

                TiposAusencia = await _queryService.GetAllAsync();

                _logger.LogInformation("Carregados {Count} tipos de ausência", TiposAusencia.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar lista de tipos de ausência");
                TempData["ErrorMessage"] = "Erro ao carregar tipos de ausência. Tente novamente.";
                TiposAusencia = new List<TipoAusenciaViewModel>();
                return Page();
            }
        }

        #endregion

        #region POST Handler - Desativar

        public async Task<IActionResult> OnPostDesativarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Desativando tipo de ausência: {TipoAusenciaId}", id);

                var result = await _commandService.DesativarAsync(id);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Tipo de ausência desativado com sucesso: {TipoAusenciaId}", id);
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _logger.LogWarning("Falha ao desativar tipo de ausência: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar tipo de ausência: {TipoAusenciaId}", id);
                TempData["ErrorMessage"] = "Erro interno ao desativar tipo de ausência.";
                return RedirectToPage();
            }
        }

        #endregion
    }
}
