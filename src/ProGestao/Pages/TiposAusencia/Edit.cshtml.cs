using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Pages.TiposAusencia
{
    /// <summary>
    /// PageModel para edição de tipos de ausência
    /// </summary>
    public class EditModel : PageModel
    {
        #region Dependencies

        private readonly ITipoAusenciaQueryService _queryService;
        private readonly ITipoAusenciaCommandService _commandService;
        private readonly ILogger<EditModel> _logger;

        #endregion

        #region Constructor

        public EditModel(
            ITipoAusenciaQueryService queryService,
            ITipoAusenciaCommandService commandService,
            ILogger<EditModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public TipoAusenciaViewModel TipoAusencia { get; set; } = new TipoAusenciaViewModel();

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando tipo de ausência {TipoAusenciaId} para edição", id);

                var tipo = await _queryService.GetByIdAsync(id);
                if (tipo == null)
                {
                    _logger.LogWarning("Tipo de ausência não encontrado: {TipoAusenciaId}", id);
                    TempData["ErrorMessage"] = "Tipo de ausência não encontrado.";
                    return RedirectToPage("./Index");
                }

                TipoAusencia = tipo;

                _logger.LogInformation("Tipo de ausência {TipoAusenciaId} carregado para edição", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipo de ausência {TipoAusenciaId} para edição", id);
                TempData["ErrorMessage"] = "Erro ao carregar tipo de ausência. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Atualizando tipo de ausência: {TipoAusenciaId}", TipoAusencia.Id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para tipo de ausência {TipoAusenciaId}", TipoAusencia.Id);
                    return Page();
                }

                var result = await _commandService.UpdateAsync(TipoAusencia);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Tipo de ausência atualizado com sucesso: {TipoAusenciaId}", TipoAusencia.Id);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na atualização do tipo de ausência: {Message}", result.Message);

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar tipo de ausência: {TipoAusenciaId}", TipoAusencia.Id);
                TempData["ErrorMessage"] = "Erro interno ao atualizar tipo de ausência. Tente novamente.";
                return Page();
            }
        }

        #endregion
    }
}
