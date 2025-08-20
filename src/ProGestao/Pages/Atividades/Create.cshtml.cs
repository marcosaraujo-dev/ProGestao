using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Extensions;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;


namespace ProGestao.Pages.Atividades
{
    /// <summary>
    /// PageModel refatorado para criação de atividades
    /// Reutiliza a mesma estrutura da edição
    /// </summary>
    public class CreateModel : PageModel
    {
        #region Dependencies

        private readonly IAtividadeCommandService _commandService;
        private readonly ILookupService _lookupService;
        private readonly ILogger<CreateModel> _logger;

        #endregion

        #region Constructor

        public CreateModel(
            IAtividadeCommandService commandService,
            ILookupService lookupService,
            ILogger<CreateModel> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        [BindProperty]
        public AtividadeViewModel Atividade { get; set; } = new AtividadeViewModel();

        public IList<ProjetoViewModel> Projetos { get; set; } = new List<ProjetoViewModel>();
        public IList<UsuarioViewModel> Usuarios { get; set; } = new List<UsuarioViewModel>();
        public IList<TipoAtividadeViewModel> TiposAtividade { get; set; } = new List<TipoAtividadeViewModel>();
        public IList<StatusAtividadeViewModel> StatusAtividades { get; set; } = new List<StatusAtividadeViewModel>();

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando criação de nova atividade");

                // Inicializar com valores padrão
                Atividade.DataInicio = DateTime.Today;
                Atividade.Prioridade = 3; // Normal

                await LoadDropdownDataAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar criação de atividade");
                TempData["ErrorMessage"] = "Erro ao carregar formulário. Tente novamente.";
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando criação de atividade: {Nome}", Atividade.Nome);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para criação de atividade");
                    await LoadDropdownDataAsync();
                    return Page();
                }

                var result = await _commandService.CreateAtividadeAsync(Atividade);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Atividade criada com sucesso: ID {AtividadeId}", result.Data);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na criação da atividade: {Message}", result.Message);

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
                _logger.LogError(ex, "Erro inesperado ao criar atividade");
                TempData["ErrorMessage"] = "Erro interno ao criar atividade. Tente novamente.";
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

                var projetosAtivosTask = _lookupService.GetProjetosAtivosAsync();
                var usuariosAtivosTask = _lookupService.GetUsuariosAtivosAsync();  
                var tiposAtividadeTask = _lookupService.GetTiposAtividadeAsync();
                var statusAtividadesTask = _lookupService.GetStatusAtividadesAsync();


                Projetos = await projetosAtivosTask;
                Usuarios = await usuariosAtivosTask;
                TiposAtividade = await tiposAtividadeTask;
                StatusAtividades = await statusAtividadesTask;

                _logger.LogDebug("Dados dos dropdowns carregados: {ProjetosCount} projetos, {UsuariosCount} usuários, {TiposCount} tipos, {StatusCount} status",
                    Projetos.Count, Usuarios.Count, TiposAtividade.Count, StatusAtividades.Count);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para dropdowns na criação");

                Projetos ??= new List<ProjetoViewModel>();
                Usuarios ??= new List<UsuarioViewModel>();
                TiposAtividade ??= new List<TipoAtividadeViewModel>();
                StatusAtividades ??= new List<StatusAtividadeViewModel>();
            }
        }

        #endregion
    }
}