using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Projetos
{

    /// <summary>
    /// PageModel refatorado para cria��o de projetos
    /// </summary>
    public class CreateModel : PageModel
    {
        #region Dependencies

        private readonly IProjetoCommandService _commandService;
        private readonly IProjetoLookupService _lookupService;
        private readonly ILogger<CreateModel> _logger;

        #endregion

        #region Constructor

        public CreateModel(
            IProjetoCommandService commandService,
            IProjetoLookupService lookupService,
            ILogger<CreateModel> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public ProjetoViewModel Projeto { get; set; } = new ProjetoViewModel();

        public IList<StatusProjetoViewModel> StatusProjetos { get; set; } = new List<StatusProjetoViewModel>();
        public IList<UsuarioLookupViewModel> Responsaveis { get; set; } = new List<UsuarioLookupViewModel>();
        
        // Propriedades para compatibilidade com views
        public IList<StatusProjetoViewModel> StatusSelectList => StatusProjetos;
        public IList<UsuarioLookupViewModel> ResponsaveisSelectList => Responsaveis;

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando cria��o de novo projeto");

                Projeto.DataInicio = DateTime.Today;

                await LoadDropdownDataAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar cria��o de projeto");
                TempData["ErrorMessage"] = "Erro ao carregar formul�rio. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando cria��o de projeto: {Nome}", Projeto.Nome);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inv�lido para cria��o de projeto");
                    await LoadDropdownDataAsync();
                    return Page();
                }

                var result = await _commandService.CreateProjetoAsync(Projeto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Projeto criado com sucesso: ID {ProjetoId}", result.Data);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na cria��o do projeto: {Message}", result.Message);

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    await LoadDropdownDataAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar projeto");
                TempData["ErrorMessage"] = "Erro interno ao criar projeto. Tente novamente.";
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
                _logger.LogError(ex, "Erro ao carregar dados para dropdowns na cria��o");

                StatusProjetos ??= new List<StatusProjetoViewModel>();
                Responsaveis ??= new List<UsuarioLookupViewModel>();
            }
        }

        #endregion
    }
}