using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;
using System.Text; 
using ProGestao.Data;
using Microsoft.EntityFrameworkCore;

namespace ProGestao.Pages.Atividades
{
    /// <summary>
    /// PageModel para criação de atividades
    /// Implementa Separation of Concerns, Dependency Injection e SOLID principles
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

        /// <summary>
        /// Lista de projetos ativos para dropdown
        /// </summary>
        public IList<ProjetoViewModel> Projetos { get; set; } = new List<ProjetoViewModel>();

        /// <summary>
        /// Lista de usuários ativos para dropdown
        /// </summary>
        public IList<UsuarioViewModel> Usuarios { get; set; } = new List<UsuarioViewModel>();

        /// <summary>
        /// Lista de tipos de atividade para dropdown
        /// </summary>
        public IList<TipoAtividadeViewModel> TiposAtividade { get; set; } = new List<TipoAtividadeViewModel>();

        /// <summary>
        /// Lista de status de atividade para dropdown
        /// </summary>
        public IList<StatusAtividadeViewModel> StatusAtividades { get; set; } = new List<StatusAtividadeViewModel>();

        #endregion

        #region GET Handler

        /// <summary>
        /// Carrega os dados necessários para exibir o formulário de criação
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando página de criação de atividade");

                // Limpar mensagens anteriores
                ClearTempDataMessages();

                // Inicializar valores padrão da atividade
                InitializeDefaultValues();

                // Carregar dados para dropdowns
                await LoadDropdownDataAsync();

                _logger.LogInformation("Página de criação carregada com sucesso");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de criação de atividade");
                SetErrorMessage("Erro ao carregar formulário. Tente novamente.");
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        /// <summary>
        /// Processa a criação de uma nova atividade
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando criação de nova atividade: {AtividadeNome}", Atividade.Nome);

                // Validação do ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para criação de atividade");
                    await LoadDropdownDataAsync();
                    return Page();
                }

                // Executar criação usando o service
                var result = await _commandService.CreateAtividadeAsync(Atividade);

                // Tratar resultado
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Atividade criada com sucesso. ID: {AtividadeId}", result.Data);
                    SetSuccessMessage(result.Message);
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Falha na criação da atividade: {Message}", result.Message);

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
                _logger.LogError(ex, "Erro inesperado ao criar atividade");
                SetErrorMessage("Erro interno ao criar atividade. Tente novamente.");
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inicializa valores padrão para uma nova atividade
        /// </summary>
        private void InitializeDefaultValues()
        {
            Atividade = new AtividadeViewModel
            {
                DataInicio = DateTime.Now.Date.AddHours(8), // 8:00 do dia atual
                Prioridade = 3, // Prioridade Normal
                HorasEstimadas = 8 // 1 dia de trabalho padrão
            };
        }

        // <summary>
        /// Carrega dados para dropdowns de forma sequencial e com logging detalhado
        /// Resolve problemas de concorrência e timing no carregamento
        /// </summary>
        private async Task LoadDropdownDataAsync()
        {
            _logger.LogInformation("=== INICIANDO CARREGAMENTO DOS DROPDOWNS ===");

            try
            {
                // Inicializar listas vazias para evitar null reference
                Projetos = new List<ProjetoViewModel>();
                Usuarios = new List<UsuarioViewModel>();
                TiposAtividade = new List<TipoAtividadeViewModel>();
                StatusAtividades = new List<StatusAtividadeViewModel>();

                // 1. CARREGAR PROJETOS
                _logger.LogDebug("1/4 - Carregando projetos ativos...");
                try
                {
                    Projetos = await _lookupService.GetProjetosAtivosAsync();
                    _logger.LogInformation("✅ Projetos carregados: {Count} itens", Projetos.Count);

                    if (Projetos.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Nenhum projeto ativo encontrado");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar projetos");
                    Projetos = new List<ProjetoViewModel>();
                }

                // 2. CARREGAR USUÁRIOS
                _logger.LogDebug("2/4 - Carregando usuários ativos...");
                try
                {
                    Usuarios = await _lookupService.GetUsuariosAtivosAsync();
                    _logger.LogInformation("✅ Usuários carregados: {Count} itens", Usuarios.Count);

                    if (Usuarios.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Nenhum usuário ativo encontrado");
                        ModelState.AddModelError("", "Nenhum usuário ativo encontrado. Cadastre usuários primeiro.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar usuários");
                    Usuarios = new List<UsuarioViewModel>();
                }

                // 3. CARREGAR TIPOS DE ATIVIDADE
                _logger.LogDebug("3/4 - Carregando tipos de atividade...");
                try
                {
                    TiposAtividade = await _lookupService.GetTiposAtividadeAsync();
                    _logger.LogInformation("✅ Tipos de Atividade carregados: {Count} itens", TiposAtividade.Count);

                    if (TiposAtividade.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Nenhum tipo de atividade encontrado");
                        ModelState.AddModelError("", "Nenhum tipo de atividade encontrado. Configure os tipos primeiro.");
                    }
                    else
                    {
                        // Log dos tipos carregados para debug
                        foreach (var tipo in TiposAtividade.Take(3))
                        {
                            _logger.LogDebug("Tipo carregado: ID={Id}, Nome='{Nome}', Cor='{Cor}'",
                                tipo.Id, tipo.Nome, tipo.Cor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar tipos de atividade");
                    TiposAtividade = new List<TipoAtividadeViewModel>();
                }

                // 4. CARREGAR STATUS DE ATIVIDADES
                _logger.LogDebug("4/4 - Carregando status de atividades...");
                try
                {
                    StatusAtividades = await _lookupService.GetStatusAtividadesAsync();
                    _logger.LogInformation("✅ Status de Atividades carregados: {Count} itens", StatusAtividades.Count);

                    if (StatusAtividades.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Nenhum status de atividade encontrado");
                        ModelState.AddModelError("", "Nenhum status de atividade encontrado. Configure os status primeiro.");
                    }
                    else
                    {
                        // Log dos status carregados para debug
                        foreach (var status in StatusAtividades.Take(3))
                        {
                            _logger.LogDebug("Status carregado: ID={Id}, Nome='{Nome}', Cor='{Cor}', Ordem={Ordem}",
                                status.Id, status.Nome, status.Cor, status.Ordem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar status de atividades");
                    StatusAtividades = new List<StatusAtividadeViewModel>();
                }

                // RESUMO FINAL
                _logger.LogInformation("=== CARREGAMENTO CONCLUÍDO ===");
                _logger.LogInformation("📊 Resumo: {ProjetosCount} projetos, {UsuariosCount} usuários, {TiposCount} tipos, {StatusCount} status",
                    Projetos.Count, Usuarios.Count, TiposAtividade.Count, StatusAtividades.Count);

                // VALIDAÇÃO FINAL
                ValidateLoadedData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO CRÍTICO no carregamento dos dropdowns");

                // Garantir que todas as listas estão inicializadas mesmo em caso de erro crítico
                Projetos ??= new List<ProjetoViewModel>();
                Usuarios ??= new List<UsuarioViewModel>();
                TiposAtividade ??= new List<TipoAtividadeViewModel>();
                StatusAtividades ??= new List<StatusAtividadeViewModel>();

                SetErrorMessage("Erro ao carregar dados do formulário. Alguns campos podem não estar disponíveis.");
            }
        }

        // <summary>
        /// Valida se os dados essenciais foram carregados corretamente
        /// </summary>
        private void ValidateLoadedData()
        {
            var errors = new List<string>();

            if (Usuarios.Count == 0)
                errors.Add("Responsável: Nenhum usuário disponível");

            if (TiposAtividade.Count == 0)
                errors.Add("Tipo de Atividade: Nenhum tipo disponível");

            if (StatusAtividades.Count == 0)
                errors.Add("Status: Nenhum status disponível");

            if (errors.Any())
            {
                var errorMessage = "Campos obrigatórios sem dados: " + string.Join(", ", errors);
                _logger.LogWarning("⚠️ Validação falhou: {ErrorMessage}", errorMessage);
                ModelState.AddModelError("", errorMessage);
            }
            else
            {
                _logger.LogInformation("✅ Validação dos dados carregados: SUCESSO");
            }
        }

        /// <summary>
        /// Valida se existem dados mínimos necessários para criar uma atividade
        /// </summary>
        private void ValidateRequiredData()
        {
            var warnings = new List<string>();

            if (!Usuarios.Any())
            {
                warnings.Add("Nenhum usuário ativo encontrado. Cadastre usuários antes de criar atividades.");
            }

            if (!TiposAtividade.Any())
            {
                warnings.Add("Nenhum tipo de atividade encontrado. Configure os tipos de atividade.");
            }

            if (!StatusAtividades.Any())
            {
                warnings.Add("Nenhum status de atividade encontrado. Configure os status de atividade.");
            }

            if (warnings.Any())
            {
                foreach (var warning in warnings)
                {
                    ModelState.AddModelError("", warning);
                }

                _logger.LogWarning("Dados insuficientes para criação de atividade: {Warnings}", string.Join("; ", warnings));
            }
        }

        /// <summary>
        /// Limpa mensagens temporárias anteriores
        /// </summary>
        private void ClearTempDataMessages()
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("WarningMessage");
        }

        /// <summary>
        /// Define mensagem de sucesso para exibição
        /// </summary>
        private void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Define mensagem de erro para exibição
        /// </summary>
        private void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        #endregion
    }
}