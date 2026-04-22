using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Pages.Ausencias
{
    /// <summary>
    /// PageModel para criação de ausências
    /// </summary>
    public class CreateModel : PageModel
    {
        #region Dependencies

        private readonly IAusenciaCommandService _commandService;
        private readonly ITipoAusenciaQueryService _tipoAusenciaQueryService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<CreateModel> _logger;

        #endregion

        #region Constructor

        public CreateModel(
            IAusenciaCommandService commandService,
            ITipoAusenciaQueryService tipoAusenciaQueryService,
            ProGestaoContext context,
            ILogger<CreateModel> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _tipoAusenciaQueryService = tipoAusenciaQueryService ?? throw new ArgumentNullException(nameof(tipoAusenciaQueryService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public AusenciaViewModel Ausencia { get; set; } = new AusenciaViewModel();

        public SelectList UsuariosSelectList { get; set; } = default!;
        public SelectList TiposAusenciaSelectList { get; set; } = default!;

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Iniciando criação de nova ausência");
            await CarregarSelectListsAsync();
            return Page();
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Criando ausência para usuário: {UsuarioId}", Ausencia.UsuarioId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para criação de ausência");
                    await CarregarSelectListsAsync();
                    return Page();
                }

                var result = await _commandService.CreateAsync(Ausencia);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Ausência criada com sucesso: ID {AusenciaId}", result.Data);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na criação da ausência: {Message}", result.Message);

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    await CarregarSelectListsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar ausência");
                TempData["ErrorMessage"] = "Erro interno ao criar ausência. Tente novamente.";
                await CarregarSelectListsAsync();
                return Page();
            }
        }

        #endregion

        #region Private Methods

        private async Task CarregarSelectListsAsync()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .AsNoTracking()
                .ToListAsync();

            UsuariosSelectList = new SelectList(usuarios, "Id", "Nome");

            var tiposAusencia = await _tipoAusenciaQueryService.GetAtivosAsync();
            TiposAusenciaSelectList = new SelectList(tiposAusencia, "Id", "Nome");
        }

        #endregion
    }
}
