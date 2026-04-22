using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Ausencia;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Pages.Ausencias
{
    /// <summary>
    /// PageModel para listagem de ausências com filtros
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Dependencies

        private readonly IAusenciaQueryService _queryService;
        private readonly IAusenciaCommandService _commandService;
        private readonly ITipoAusenciaQueryService _tipoAusenciaQueryService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;

        #endregion

        #region Constructor

        public IndexModel(
            IAusenciaQueryService queryService,
            IAusenciaCommandService commandService,
            ITipoAusenciaQueryService tipoAusenciaQueryService,
            ProGestaoContext context,
            ILogger<IndexModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _tipoAusenciaQueryService = tipoAusenciaQueryService ?? throw new ArgumentNullException(nameof(tipoAusenciaQueryService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public IList<AusenciaViewModel> Ausencias { get; set; } = new List<AusenciaViewModel>();

        public SelectList UsuariosSelectList { get; set; } = default!;
        public SelectList TiposAusenciaSelectList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int? FiltroUsuarioId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroTipoAusenciaId { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? FiltroDataInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? FiltroDataFim { get; set; }

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando lista de ausências com filtros");

                await CarregarSelectListsAsync();

                Ausencias = await _queryService.GetByFiltroAsync(
                    usuarioId: FiltroUsuarioId,
                    tipoAusenciaId: FiltroTipoAusenciaId,
                    dataInicio: FiltroDataInicio,
                    dataFim: FiltroDataFim);

                _logger.LogInformation("Carregadas {Count} ausências", Ausencias.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar lista de ausências");
                TempData["ErrorMessage"] = "Erro ao carregar ausências. Tente novamente.";
                Ausencias = new List<AusenciaViewModel>();
                await CarregarSelectListsAsync();
                return Page();
            }
        }

        #endregion

        #region POST Handler - Desativar

        public async Task<IActionResult> OnPostDesativarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Desativando ausência: {AusenciaId}", id);

                var result = await _commandService.DesativarAsync(id);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Ausência desativada com sucesso: {AusenciaId}", id);
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _logger.LogWarning("Falha ao desativar ausência: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar ausência: {AusenciaId}", id);
                TempData["ErrorMessage"] = "Erro interno ao desativar ausência.";
                return RedirectToPage();
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
