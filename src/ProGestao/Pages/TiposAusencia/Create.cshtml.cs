using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Pages.TiposAusencia
{
    /// <summary>
    /// PageModel para criação de tipos de ausência
    /// </summary>
    public class CreateModel : PageModel
    {
        #region Dependencies

        private readonly ITipoAusenciaCommandService _commandService;
        private readonly ILogger<CreateModel> _logger;

        #endregion

        #region Constructor

        public CreateModel(
            ITipoAusenciaCommandService commandService,
            ILogger<CreateModel> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public TipoAusenciaViewModel TipoAusencia { get; set; } = new TipoAusenciaViewModel();

        #endregion

        #region GET Handler

        public IActionResult OnGet()
        {
            _logger.LogInformation("Iniciando criação de novo tipo de ausência");
            return Page();
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Criando tipo de ausência: {Nome}", TipoAusencia.Nome);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para criação de tipo de ausência");
                    return Page();
                }

                var result = await _commandService.CreateAsync(TipoAusencia);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Tipo de ausência criado com sucesso: ID {TipoAusenciaId}", result.Data);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na criação do tipo de ausência: {Message}", result.Message);

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar tipo de ausência");
                TempData["ErrorMessage"] = "Erro interno ao criar tipo de ausência. Tente novamente.";
                return Page();
            }
        }

        #endregion
    }
}
