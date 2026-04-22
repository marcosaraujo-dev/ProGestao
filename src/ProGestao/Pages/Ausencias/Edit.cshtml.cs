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
    /// PageModel para edição de ausências
    /// </summary>
    public class EditModel : PageModel
    {
        #region Dependencies

        private readonly IAusenciaQueryService _queryService;
        private readonly IAusenciaCommandService _commandService;
        private readonly ITipoAusenciaQueryService _tipoAusenciaQueryService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<EditModel> _logger;

        #endregion

        #region Constructor

        public EditModel(
            IAusenciaQueryService queryService,
            IAusenciaCommandService commandService,
            ITipoAusenciaQueryService tipoAusenciaQueryService,
            ProGestaoContext context,
            ILogger<EditModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando ausência {AusenciaId} para edição", id);

                var ausencia = await _queryService.GetByIdAsync(id);
                if (ausencia == null)
                {
                    _logger.LogWarning("Ausência não encontrada: {AusenciaId}", id);
                    TempData["ErrorMessage"] = "Ausência não encontrada.";
                    return RedirectToPage("./Index");
                }

                Ausencia = ausencia;
                await CarregarSelectListsAsync();

                _logger.LogInformation("Ausência {AusenciaId} carregada para edição", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar ausência {AusenciaId} para edição", id);
                TempData["ErrorMessage"] = "Erro ao carregar ausência. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Atualizando ausência: {AusenciaId}", Ausencia.Id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para ausência {AusenciaId}", Ausencia.Id);
                    await CarregarSelectListsAsync();
                    return Page();
                }

                var result = await _commandService.UpdateAsync(Ausencia);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Ausência atualizada com sucesso: {AusenciaId}", Ausencia.Id);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na atualização da ausência: {Message}", result.Message);

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
                _logger.LogError(ex, "Erro inesperado ao atualizar ausência: {AusenciaId}", Ausencia.Id);
                TempData["ErrorMessage"] = "Erro interno ao atualizar ausência. Tente novamente.";
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
