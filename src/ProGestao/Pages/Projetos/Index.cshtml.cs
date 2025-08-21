using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Projetos
{
    /// <summary>
    /// PageModel refatorado para listagem de projetos
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Dependencies

        private readonly IProjetoQueryService _queryService;
        private readonly IProjetoLookupService _lookupService;
        private readonly ILogger<IndexModel> _logger;

        #endregion

        #region Constructor

        public IndexModel(
            IProjetoQueryService queryService,
            IProjetoLookupService lookupService,
            ILogger<IndexModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public IList<ProjetoViewModel> Projetos { get; set; } = new List<ProjetoViewModel>();
        public IList<StatusProjetoViewModel> StatusProjetos { get; set; } = new List<StatusProjetoViewModel>();
        public IList<UsuarioLookupViewModel> Responsaveis { get; set; } = new List<UsuarioLookupViewModel>();

        [BindProperty(SupportsGet = true)]
        public ProjetoFiltroViewModel Filtros { get; set; } = new ProjetoFiltroViewModel();
        
        [BindProperty(SupportsGet = true)]
        public string? FiltroStatus { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? FiltroResponsavel { get; set; }

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando lista de projetos com filtros: {@Filtros}", Filtros);

                await LoadDropdownDataAsync();

                if (Filtros.TemFiltros)
                {
                    Projetos = await _queryService.GetProjetosByFiltroAsync(
                        Filtros.Status,
                        Filtros.ResponsavelId,
                        Filtros.Nome);
                }
                else
                {
                    Projetos = await _queryService.GetProjetosAsync();
                }

                _logger.LogInformation("Carregados {Count} projetos", Projetos.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar lista de projetos");
                TempData["ErrorMessage"] = "Erro ao carregar projetos. Tente novamente.";

                Projetos = new List<ProjetoViewModel>();
                await LoadDropdownDataAsync();

                return Page();
            }
        }

        #endregion

        #region Private Methods

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                _logger.LogDebug("Carregando dados para dropdowns");

                var statusProjetosTask = _lookupService.GetStatusProjetosAsync();
                var responsaveisTask = _lookupService.GetResponsaveisAsync();

                StatusProjetos = await statusProjetosTask;
                Responsaveis = await responsaveisTask;

                _logger.LogDebug("Dados dos dropdowns carregados: {StatusCount} status, {ResponsaveisCount} respons�veis",
                    StatusProjetos.Count, Responsaveis.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para filtros");

                StatusProjetos ??= new List<StatusProjetoViewModel>();
                Responsaveis ??= new List<UsuarioLookupViewModel>();
            }
        }

        #endregion
    }
}
