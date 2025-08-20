using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Atividades
{
    /// <summary>
    /// PageModel refatorado para edição de atividades
    /// Implementa Separation of Concerns e Dependency Injection
    /// </summary>
    public class EditModel : PageModel
    {
        #region Dependencies

        private readonly IAtividadeQueryService _queryService;
        private readonly IAtividadeCommandService _commandService;
        private readonly ILookupService _lookupService;
        private readonly ILogger<EditModel> _logger;

        #endregion

        #region Constructor

        public EditModel(
            IAtividadeQueryService queryService,
            IAtividadeCommandService commandService,
            ILookupService lookupService,
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
        public AtividadeViewModel Atividade { get; set; } = new AtividadeViewModel();

        // Listas para dropdowns
        public IList<ProjetoViewModel> Projetos { get; set; } = new List<ProjetoViewModel>();
        public IList<UsuarioViewModel> Usuarios { get; set; } = new List<UsuarioViewModel>();
        public IList<TipoAtividadeViewModel> TiposAtividade { get; set; } = new List<TipoAtividadeViewModel>();
        public IList<StatusAtividadeViewModel> StatusAtividades { get; set; } = new List<StatusAtividadeViewModel>();

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando atividade {AtividadeId} para edição", id);

                // Limpar mensagens anteriores
                ClearTempDataMessages();

                // Buscar atividade
                var atividade = await _queryService.GetAtividadeComDetalhesAsync(id);
                if (atividade == null)
                {
                    _logger.LogWarning("Atividade {AtividadeId} não encontrada", id);
                    SetErrorMessage("Atividade não encontrada.");
                    return RedirectToPage("./Index");
                }

                Atividade = atividade;

                // Carregar dados para dropdowns
                await LoadDropdownDataAsync();

                _logger.LogInformation("Atividade {AtividadeId} carregada com sucesso para edição", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade {AtividadeId} para edição", id);
                SetErrorMessage("Erro ao carregar atividade. Tente novamente.");
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando atualização da atividade {AtividadeId}", Atividade.Id);

                // Validação do ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para atividade {AtividadeId}", Atividade.Id);
                    await LoadDropdownDataAsync();
                    return Page();
                }

                // Executar atualização usando o service
                var result = await _commandService.UpdateAtividadeAsync(Atividade);

                // Tratar resultado
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Atividade {AtividadeId} atualizada com sucesso", Atividade.Id);
                    SetSuccessMessage(result.Message);
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na atualização da atividade {AtividadeId}: {Message}",
                        Atividade.Id, result.Message);

                    // Adicionar erros ao ModelState
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
                _logger.LogError(ex, "Erro inesperado ao atualizar atividade {AtividadeId}", Atividade.Id);
                SetErrorMessage("Erro interno ao atualizar atividade. Tente novamente.");
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Carrega dados para dropdowns de forma assíncrona e otimizada
        /// </summary>
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
                _logger.LogError(ex, "Erro ao carregar dados para dropdowns");

                // Garantir que as listas não sejam null
                Projetos ??= new List<ProjetoViewModel>();
                Usuarios ??= new List<UsuarioViewModel>();
                TiposAtividade ??= new List<TipoAtividadeViewModel>();
                StatusAtividades ??= new List<StatusAtividadeViewModel>();

                SetErrorMessage("Erro ao carregar dados do formulário. Alguns campos podem não estar disponíveis.");
            }
        }

        /// <summary>
        /// Limpa mensagens TempData anteriores
        /// </summary>
        private void ClearTempDataMessages()
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("WarningMessage");
            TempData.Remove("InfoMessage");
        }

        /// <summary>
        /// Define mensagem de sucesso
        /// </summary>
        private void SetSuccessMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["SuccessMessage"] = message.Trim();
            }
        }

        /// <summary>
        /// Define mensagem de erro
        /// </summary>
        private void SetErrorMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = message.Trim();
            }
        }

        /// <summary>
        /// Define mensagem de aviso
        /// </summary>
        private void SetWarningMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["WarningMessage"] = message.Trim();
            }
        }

        #endregion
    }
}