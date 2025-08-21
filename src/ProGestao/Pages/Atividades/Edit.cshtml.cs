using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;
using System.Text;

namespace ProGestao.Pages.Atividades
{
    /// <summary>
    /// PageModel refatorado para edição de atividades
    /// Implementa Separation of Concerns e Dependency Injection
    /// Versão melhorada que resolve problemas de carregamento das combos
    /// </summary>
    public class EditModel : PageModel
    {
        #region Dependencies

        private readonly IAtividadeQueryService _queryService;
        private readonly IAtividadeCommandService _commandService;
        private readonly ILookupService _lookupService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<EditModel> _logger;

        #endregion

        #region Constructor

        public EditModel(
            IAtividadeQueryService queryService,
            IAtividadeCommandService commandService,
            ILookupService lookupService,
            ProGestaoContext context,
            ILogger<EditModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
        /// Carrega uma atividade específica para edição
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Carregando atividade {AtividadeId} para edição", id);

                // Limpar mensagens anteriores
                ClearTempDataMessages();

                // Buscar atividade
                var atividade = await _queryService.GetAtividadeComDetalhesAsync(id);
                if (atividade == null)
                {
                    _logger.LogWarning("❌ Atividade {AtividadeId} não encontrada", id);
                    SetErrorMessage("Atividade não encontrada.");
                    return RedirectToPage("./Index");
                }

                Atividade = atividade;
                _logger.LogInformation("✅ Atividade carregada: '{AtividadeNome}' (ID: {AtividadeId})",
                    atividade.Nome, atividade.Id);

                // Carregar dados para dropdowns com método melhorado
                await LoadDropdownDataAsync();

                _logger.LogInformation("🎉 Atividade {AtividadeId} carregada com sucesso para edição", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Erro ao carregar atividade {AtividadeId} para edição", id);
                SetErrorMessage("Erro ao carregar atividade. Tente novamente.");
                return RedirectToPage("./Index");
            }
        }

        #endregion

        #region POST Handler

        /// <summary>
        /// Processa a atualização de uma atividade
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("💾 Iniciando atualização da atividade {AtividadeId}", Atividade.Id);

                // Validação do ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("⚠️ ModelState inválido para atividade {AtividadeId}", Atividade.Id);
                    await LoadDropdownDataAsync();
                    return Page();
                }

                // Executar atualização usando o service
                var result = await _commandService.UpdateAtividadeAsync(Atividade);

                // Tratar resultado
                if (result.IsSuccess)
                {
                    _logger.LogInformation("🎉 Atividade {AtividadeId} atualizada com sucesso", Atividade.Id);
                    SetSuccessMessage(result.Message);
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("❌ Falha na atualização da atividade {AtividadeId}: {Message}",
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
                _logger.LogError(ex, "💥 Erro inesperado ao atualizar atividade {AtividadeId}", Atividade.Id);
                SetErrorMessage("Erro interno ao atualizar atividade. Tente novamente.");
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        #endregion

        #region Private Methods - Carregamento de Dados

        /// <summary>
        /// Carrega dados para dropdowns de forma sequencial e com logging detalhado
        /// Resolve problemas de concorrência e timing no carregamento
        /// </summary>
        private async Task LoadDropdownDataAsync()
        {
            _logger.LogInformation("🔄 === INICIANDO CARREGAMENTO DOS DROPDOWNS (EDIT) ===");

            try
            {
                // Inicializar listas vazias para evitar null reference
                Projetos = new List<ProjetoViewModel>();
                Usuarios = new List<UsuarioViewModel>();
                TiposAtividade = new List<TipoAtividadeViewModel>();
                StatusAtividades = new List<StatusAtividadeViewModel>();

                // 1. CARREGAR PROJETOS
                _logger.LogDebug("📁 1/4 - Carregando projetos ativos...");
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
                    // Tenta carregamento direto como fallback
                    await LoadProjetosDiretoAsync();
                }

                // 2. CARREGAR USUÁRIOS
                _logger.LogDebug("👥 2/4 - Carregando usuários ativos...");
                try
                {
                    Usuarios = await _lookupService.GetUsuariosAtivosAsync();
                    _logger.LogInformation("✅ Usuários carregados: {Count} itens", Usuarios.Count);

                    if (Usuarios.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Nenhum usuário ativo encontrado");
                        ModelState.AddModelError("", "Nenhum usuário ativo encontrado. Cadastre usuários primeiro.");
                    }
                    else
                    {
                        // Log do usuário selecionado para debug
                        var usuarioSelecionado = Usuarios.FirstOrDefault(u => u.Id == Atividade.UsuarioId);
                        if (usuarioSelecionado != null)
                        {
                            _logger.LogDebug("👤 Usuário atual da atividade: {UsuarioNome} (ID: {UsuarioId})",
                                usuarioSelecionado.Nome, usuarioSelecionado.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar usuários");
                    Usuarios = new List<UsuarioViewModel>();
                    // Tenta carregamento direto como fallback
                    await LoadUsuariosDiretoAsync();
                }

                // 3. CARREGAR TIPOS DE ATIVIDADE
                _logger.LogDebug("🏷️ 3/4 - Carregando tipos de atividade...");
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
                        // Log do tipo selecionado para debug
                        var tipoSelecionado = TiposAtividade.FirstOrDefault(t => t.Id == Atividade.TipoAtividadeId);
                        if (tipoSelecionado != null)
                        {
                            _logger.LogDebug("🏷️ Tipo atual da atividade: {TipoNome} (ID: {TipoId})",
                                tipoSelecionado.Nome, tipoSelecionado.Id);
                        }

                        // Log dos primeiros tipos para debug
                        foreach (var tipo in TiposAtividade.Take(3))
                        {
                            _logger.LogDebug("🏷️ Tipo disponível: ID={Id}, Nome='{Nome}', Cor='{Cor}'",
                                tipo.Id, tipo.Nome, tipo.Cor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar tipos de atividade");
                    TiposAtividade = new List<TipoAtividadeViewModel>();
                    // Tenta carregamento direto como fallback
                    await LoadTiposDiretoAsync();
                }

                // 4. CARREGAR STATUS DE ATIVIDADES
                _logger.LogDebug("📊 4/4 - Carregando status de atividades...");
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
                        // Log do status selecionado para debug
                        var statusSelecionado = StatusAtividades.FirstOrDefault(s => s.Id == Atividade.StatusId);
                        if (statusSelecionado != null)
                        {
                            _logger.LogDebug("📊 Status atual da atividade: {StatusNome} (ID: {StatusId})",
                                statusSelecionado.Nome, statusSelecionado.Id);
                        }

                        // Log dos primeiros status para debug
                        foreach (var status in StatusAtividades.Take(3))
                        {
                            _logger.LogDebug("📊 Status disponível: ID={Id}, Nome='{Nome}', Cor='{Cor}', Ordem={Ordem}",
                                status.Id, status.Nome, status.Cor, status.Ordem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao carregar status de atividades");
                    StatusAtividades = new List<StatusAtividadeViewModel>();
                    // Tenta carregamento direto como fallback
                    await LoadStatusDiretoAsync();
                }

                // RESUMO FINAL
                _logger.LogInformation("🎯 === CARREGAMENTO CONCLUÍDO (EDIT) ===");
                _logger.LogInformation("📊 Resumo: {ProjetosCount} projetos, {UsuariosCount} usuários, {TiposCount} tipos, {StatusCount} status",
                    Projetos.Count, Usuarios.Count, TiposAtividade.Count, StatusAtividades.Count);

                // VALIDAÇÃO FINAL
                ValidateLoadedData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERRO CRÍTICO no carregamento dos dropdowns (EDIT)");

                // Garantir que todas as listas estão inicializadas mesmo em caso de erro crítico
                Projetos ??= new List<ProjetoViewModel>();
                Usuarios ??= new List<UsuarioViewModel>();
                TiposAtividade ??= new List<TipoAtividadeViewModel>();
                StatusAtividades ??= new List<StatusAtividadeViewModel>();

                SetErrorMessage("Erro ao carregar dados do formulário. Alguns campos podem não estar disponíveis.");
            }
        }

        /// <summary>
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

        #endregion

        #region Private Methods - Carregamento Direto (Fallback)

        /// <summary>
        /// Carregamento direto de projetos como fallback
        /// </summary>
        private async Task LoadProjetosDiretoAsync()
        {
            try
            {
                _logger.LogInformation("🔧 Usando carregamento direto para projetos");
                Projetos = await _context.Projetos
                    .Include(p => p.Status)
                    .Include(p => p.Responsavel)
                    .Where(p => p.Status.Nome != "Cancelado")
                    .OrderBy(p => p.Nome)
                    .Select(p => new ProjetoViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        StatusNome = p.Status.Nome ?? string.Empty,
                        StatusCor = p.Status.Cor ?? "#6c757d"
                    })
                    .ToListAsync();
                _logger.LogInformation("✅ Projetos carregados diretamente: {Count}", Projetos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no carregamento direto de projetos");
            }
        }

        /// <summary>
        /// Carregamento direto de usuários como fallback
        /// </summary>
        private async Task LoadUsuariosDiretoAsync()
        {
            try
            {
                _logger.LogInformation("🔧 Usando carregamento direto para usuários");
                Usuarios = await _context.Usuarios
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .Select(u => new UsuarioViewModel
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Cargo = u.Cargo,
                        EquipeNome = u.Equipe.Nome ?? string.Empty,
                        Ativo = u.Ativo
                    })
                    .ToListAsync();
                _logger.LogInformation("✅ Usuários carregados diretamente: {Count}", Usuarios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no carregamento direto de usuários");
            }
        }

        /// <summary>
        /// Carregamento direto de tipos como fallback
        /// </summary>
        private async Task LoadTiposDiretoAsync()
        {
            try
            {
                _logger.LogInformation("🔧 Usando carregamento direto para tipos de atividade");
                TiposAtividade = await _context.TiposAtividade
                    .Where(t => t.Ativo)
                    .OrderBy(t => t.Nome)
                    .Select(t => new TipoAtividadeViewModel
                    {
                        Id = t.Id,
                        Nome = t.Nome,
                        Cor = t.Cor,
                        Ativo = t.Ativo
                    })
                    .ToListAsync();
                _logger.LogInformation("✅ Tipos carregados diretamente: {Count}", TiposAtividade.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no carregamento direto de tipos");
            }
        }

        /// <summary>
        /// Carregamento direto de status como fallback
        /// </summary>
        private async Task LoadStatusDiretoAsync()
        {
            try
            {
                _logger.LogInformation("🔧 Usando carregamento direto para status");
                StatusAtividades = await _context.StatusAtividades
                    .OrderBy(s => s.Ordem)
                    .Select(s => new StatusAtividadeViewModel
                    {
                        Id = s.Id,
                        Nome = s.Nome,
                        Cor = s.Cor,
                        Ordem = s.Ordem
                    })
                    .ToListAsync();
                _logger.LogInformation("✅ Status carregados diretamente: {Count}", StatusAtividades.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no carregamento direto de status");
            }
        }

        #endregion

        #region Private Methods - Utilidades

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

        #region Debug Methods (Remover em produção)

        /// <summary>
        /// Método de diagnóstico completo - USE APENAS PARA DEBUG
        /// </summary>
        private async Task<string> DiagnosticoCompletoAsync()
        {
            var diagnostic = new StringBuilder();
            diagnostic.AppendLine("🔍 === DIAGNÓSTICO COMPLETO (EDIT) ===");

            // Teste 1: Verificar injeção de dependência
            diagnostic.AppendLine($"LookupService injetado: {(_lookupService != null ? "✅ SIM" : "❌ NÃO")}");
            diagnostic.AppendLine($"Context injetado: {(_context != null ? "✅ SIM" : "❌ NÃO")}");

            // Teste 2: Verificar banco de dados
            try
            {
                var usuariosCount = await _context.Usuarios.CountAsync(u => u.Ativo);
                diagnostic.AppendLine($"Usuários ativos no banco: {usuariosCount}");

                var tiposCount = await _context.TiposAtividade.CountAsync(t => t.Ativo);
                diagnostic.AppendLine($"Tipos ativos no banco: {tiposCount}");

                var statusCount = await _context.StatusAtividades.CountAsync();
                diagnostic.AppendLine($"Status no banco: {statusCount}");

                var projetosCount = await _context.Projetos
                    .Include(p => p.Status)
                    .CountAsync(p => p.Status.Nome != "Cancelado");
                diagnostic.AppendLine($"Projetos ativos no banco: {projetosCount}");
            }
            catch (Exception ex)
            {
                diagnostic.AppendLine($"❌ Erro ao acessar banco: {ex.Message}");
            }

            // Teste 3: Testar LookupService
            if (_lookupService != null)
            {
                try
                {
                    var usuarios = await _lookupService.GetUsuariosAtivosAsync();
                    diagnostic.AppendLine($"LookupService.GetUsuarios: ✅ {usuarios.Count} itens");

                    var tipos = await _lookupService.GetTiposAtividadeAsync();
                    diagnostic.AppendLine($"LookupService.GetTipos: ✅ {tipos.Count} itens");

                    var status = await _lookupService.GetStatusAtividadesAsync();
                    diagnostic.AppendLine($"LookupService.GetStatus: ✅ {status.Count} itens");

                    var projetos = await _lookupService.GetProjetosAtivosAsync();
                    diagnostic.AppendLine($"LookupService.GetProjetos: ✅ {projetos.Count} itens");
                }
                catch (Exception ex)
                {
                    diagnostic.AppendLine($"❌ Erro no LookupService: {ex.Message}");
                }
            }

            diagnostic.AppendLine("🔍 === FIM DO DIAGNÓSTICO ===");
            return diagnostic.ToString();
        }

        #endregion
    }
}