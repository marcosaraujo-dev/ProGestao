using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Projetos
{

    /// <summary>
    /// PageModel refatorado para edi��o de projetos
    /// Implementa Separation of Concerns e Dependency Injection
    /// </summary>
    public class EditModel : PageModel
    {
        #region Dependencies

        private readonly IProjetoQueryService _queryService;
        private readonly IProjetoCommandService _commandService;
        private readonly IProjetoLookupService _lookupService;
        private readonly ILogger<EditModel> _logger;

        #endregion

        #region Constructor

        public EditModel(
            IProjetoQueryService queryService,
            IProjetoCommandService commandService,
            IProjetoLookupService lookupService,
            ILogger<EditModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public ProjetoViewModel Projeto { get; set; } = new ProjetoViewModel();

        // Listas para dropdowns
        public IList<StatusProjetoViewModel> StatusProjetos { get; set; } = new List<StatusProjetoViewModel>();
        public IList<UsuarioLookupViewModel> Responsaveis { get; set; } = new List<UsuarioLookupViewModel>();
        
        // Propriedades para compatibilidade com views
        public IList<StatusProjetoViewModel> StatusSelectList => StatusProjetos;
        public IList<UsuarioLookupViewModel> ResponsaveisSelectList => Responsaveis;

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando projeto {ProjetoId} para edi��o", id);

                ClearTempDataMessages();

                var projeto = await _queryService.GetProjetoComDetalhesAsync(id);
                if (projeto == null)
                {
                    _logger.LogWarning("Projeto {ProjetoId} n�o encontrado", id);
                    SetErrorMessage("Projeto n�o encontrado.");
                    return RedirectToPage("./Index");
                }

                Projeto = projeto;

                await LoadDropdownDataAsync();

                _logger.LogInformation("Projeto {ProjetoId} carregado com sucesso para edi��o", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar projeto {ProjetoId} para edi��o", id);
                SetErrorMessage("Erro ao carregar projeto. Tente novamente.");
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando atualiza��o do projeto {ProjetoId}", Projeto.Id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inv�lido para projeto {ProjetoId}", Projeto.Id);
                    await LoadDropdownDataAsync();
                    return Page();
                }

                var result = await _commandService.UpdateProjetoAsync(Projeto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Projeto {ProjetoId} atualizado com sucesso", Projeto.Id);
                    SetSuccessMessage(result.Message);
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na atualiza��o do projeto {ProjetoId}: {Message}",
                        Projeto.Id, result.Message);

                    if (result.Errors.Any())
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }

                    await LoadDropdownDataAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar projeto {ProjetoId}", Projeto.Id);
                SetErrorMessage("Erro interno ao atualizar projeto. Tente novamente.");
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
                _logger.LogError(ex, "Erro ao carregar dados para dropdowns");

                StatusProjetos ??= new List<StatusProjetoViewModel>();
                Responsaveis ??= new List<UsuarioLookupViewModel>();

                SetErrorMessage("Erro ao carregar dados do formul�rio. Alguns campos podem n�o estar dispon�veis.");
            }
        }

        private void ClearTempDataMessages()
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("WarningMessage");
            TempData.Remove("InfoMessage");
        }

        private void SetSuccessMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["SuccessMessage"] = message.Trim();
            }
        }

        private void SetErrorMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = message.Trim();
            }
        }

        #endregion
    }

}