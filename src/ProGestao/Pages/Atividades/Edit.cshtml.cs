using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels;

namespace ProGestao.Pages.Atividades
{
    public class EditModel : PageModel  // ← Temporariamente volta para PageModel
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ProGestaoContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public AtividadeViewModel Atividade { get; set; } = new AtividadeViewModel();

        // Listas para dropdowns
        public IList<Projeto> Projetos { get; set; } = new List<Projeto>();
        public IList<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public IList<TipoAtividade> TiposAtividade { get; set; } = new List<TipoAtividade>();
        public IList<StatusAtividade> StatusAtividades { get; set; } = new List<StatusAtividade>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Limpar TempData antigo manualmente por enquanto
            LimparTempDataAntigo();

            try
            {
                LogComContexto($"Carregando atividade {id} para edição");

                var atividade = await _context.Atividades
                    .Include(a => a.Projeto)
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Status)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atividade == null)
                {
                    LogComContexto($"Atividade {id} não encontrada", LogLevel.Warning);
                    DefinirMensagemErroCrud("carregar", "atividade");
                    return RedirectToPage("./Index");
                }

                // Mapear entidade para ViewModel
                Atividade = MapToViewModel(atividade);

                // Carregar dados para dropdowns
                await CarregarDados();

                LogComContexto($"Atividade {id} carregada com sucesso");
                return Page();
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro ao carregar atividade {id}: {ex.Message}", LogLevel.Error);
                DefinirMensagemErroCrud("carregar", "atividade");
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogComContexto($"Dados inválidos na edição da atividade {Atividade.Id}", LogLevel.Warning);
                    await CarregarDados();
                    return Page();
                }

                // Buscar atividade existente
                var atividadeExistente = await _context.Atividades
                    .Include(a => a.Projeto)
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == Atividade.Id);

                if (atividadeExistente == null)
                {
                    LogComContexto($"Atividade {Atividade.Id} não encontrada para atualização", LogLevel.Warning);
                    DefinirMensagemErro("Atividade não encontrada.");
                    return RedirectToPage("./Index");
                }

                // Validações de negócio
                var validationResult = await ValidarAtividade(Atividade, atividadeExistente.Id);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    await CarregarDados();
                    return Page();
                }

                // Preservar dados de auditoria
                var dataOriginalCriacao = atividadeExistente.DataCriacao;
                var statusAnterior = atividadeExistente.StatusId;

                // Atualizar campos
                MapViewModelToEntity(Atividade, atividadeExistente);

                // Preservar dados de auditoria
                atividadeExistente.DataCriacao = dataOriginalCriacao;
                atividadeExistente.DataAtualizacao = DateTime.Now;

                // Log de mudança de status
                if (statusAnterior != Atividade.StatusId)
                {
                    await LogarMudancaStatus(statusAnterior, Atividade.StatusId, Atividade.Id);
                }

                // Salvar alterações
                await _context.SaveChangesAsync();

                LogComContexto($"Atividade '{atividadeExistente.Nome}' atualizada com sucesso");
                DefinirMensagemSucessoCrud("atualizar", "Atividade", atividadeExistente.Nome);

                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogComContexto($"Erro de concorrência ao atualizar atividade {Atividade.Id}: {ex.Message}", LogLevel.Error);

                if (!AtividadeExists(Atividade.Id))
                {
                    DefinirMensagemErro("A atividade foi excluída por outro usuário.");
                    return RedirectToPage("./Index");
                }
                else
                {
                    DefinirMensagemAviso("A atividade foi modificada por outro usuário. Recarregue a página e tente novamente.");
                    await CarregarDados();
                    return Page();
                }
            }
            catch (DbUpdateException ex)
            {
                LogComContexto($"Erro de banco de dados ao atualizar atividade {Atividade.Id}: {ex.Message}", LogLevel.Error);
                DefinirMensagemErroCrud("atualizar", "atividade");
                await CarregarDados();
                return Page();
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro inesperado ao atualizar atividade {Atividade.Id}: {ex.Message}", LogLevel.Error);
                DefinirMensagemErroCrud("atualizar", "atividade");
                await CarregarDados();
                return Page();
            }
        }

        #region Métodos da BasePageModel (temporariamente aqui)

        /// <summary>
        /// Limpa mensagens TempData antigas
        /// </summary>
        private void LimparTempDataAntigo()
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("WarningMessage");
            TempData.Remove("InfoMessage");
        }

        /// <summary>
        /// Define mensagem de sucesso
        /// </summary>
        private void DefinirMensagemSucesso(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["SuccessMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define mensagem de erro
        /// </summary>
        private void DefinirMensagemErro(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["ErrorMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define mensagem de aviso
        /// </summary>
        private void DefinirMensagemAviso(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["WarningMessage"] = mensagem.Trim();
            }
        }

        /// <summary>
        /// Define mensagem de sucesso para operações CRUD
        /// </summary>
        private void DefinirMensagemSucessoCrud(string operacao, string entidade, string nome = null)
        {
            var nomeItem = !string.IsNullOrWhiteSpace(nome) ? $" '{nome}'" : "";
            var mensagem = operacao.ToLower() switch
            {
                "criar" or "criado" or "criada" => $"{entidade}{nomeItem} criado com sucesso!",
                "atualizar" or "atualizado" or "atualizada" or "editar" => $"{entidade}{nomeItem} atualizado com sucesso!",
                "excluir" or "excluído" or "excluída" or "deletar" => $"{entidade}{nomeItem} excluído com sucesso!",
                _ => $"{entidade}{nomeItem} processado com sucesso!"
            };

            DefinirMensagemSucesso(mensagem);
        }

        /// <summary>
        /// Define mensagem de erro para operações CRUD
        /// </summary>
        private void DefinirMensagemErroCrud(string operacao, string entidade)
        {
            var mensagem = operacao.ToLower() switch
            {
                "criar" or "criado" or "criada" => $"Erro ao criar {entidade.ToLower()}. Tente novamente.",
                "atualizar" or "atualizado" or "atualizada" or "editar" => $"Erro ao atualizar {entidade.ToLower()}. Tente novamente.",
                "excluir" or "excluído" or "excluída" or "deletar" => $"Erro ao excluir {entidade.ToLower()}. Tente novamente.",
                "carregar" => $"Erro ao carregar {entidade.ToLower()}. Tente novamente.",
                _ => $"Erro ao processar {entidade.ToLower()}. Tente novamente."
            };

            DefinirMensagemErro(mensagem);
        }

        /// <summary>
        /// Log com contexto da página
        /// </summary>
        private void LogComContexto(string message, LogLevel level = LogLevel.Information)
        {
            var paginaAtual = GetType().Name.Replace("Model", "");
            var mensagemCompleta = $"[{paginaAtual}] {message}";

            switch (level)
            {
                case LogLevel.Error:
                    _logger.LogError(mensagemCompleta);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(mensagemCompleta);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(mensagemCompleta);
                    break;
                default:
                    _logger.LogInformation(mensagemCompleta);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Loga mudança de status da atividade
        /// </summary>
        private async Task LogarMudancaStatus(int statusAnteriorId, int statusNovoId, int atividadeId)
        {
            try
            {
                var statusAnteriorNome = await _context.StatusAtividades
                    .Where(s => s.Id == statusAnteriorId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                var statusNovoNome = await _context.StatusAtividades
                    .Where(s => s.Id == statusNovoId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                LogComContexto($"Status da atividade {atividadeId} alterado de '{statusAnteriorNome}' para '{statusNovoNome}'");
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro ao logar mudança de status: {ex.Message}", LogLevel.Warning);
            }
        }

        private async Task CarregarDados()
        {
            try
            {
                // Carregar projetos ativos
                Projetos = await _context.Projetos
                    .Include(p => p.Status)
                    .Where(p => p.Status.Nome != "Cancelado")
                    .OrderBy(p => p.Nome)
                    .ToListAsync();

                // Carregar usuários ativos
                Usuarios = await _context.Usuarios
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                // Carregar tipos de atividade
                TiposAtividade = await _context.TiposAtividade
                    .OrderBy(t => t.Nome)
                    .ToListAsync();

                // Carregar status das atividades
                StatusAtividades = await _context.StatusAtividades
                    .OrderBy(s => s.Ordem)
                    .ToListAsync();

                LogComContexto("Dados para dropdowns carregados com sucesso", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro ao carregar dados para dropdowns: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        private async Task<ValidationResult> ValidarAtividade(AtividadeViewModel atividade, int atividadeId)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            try
            {
                // Validar projeto se informado
                if (atividade.ProjetoId.HasValue)
                {
                    var validacaoProjeto = await ValidarProjeto(atividade.ProjetoId.Value);
                    if (!validacaoProjeto.IsValid)
                    {
                        result.Errors.AddRange(validacaoProjeto.Errors);
                        result.IsValid = false;
                    }
                }

                // Validar usuário
                var validacaoUsuario = await ValidarUsuario(atividade.UsuarioId);
                if (!validacaoUsuario.IsValid)
                {
                    result.Errors.AddRange(validacaoUsuario.Errors);
                    result.IsValid = false;
                }

                // Validar datas
                var validacaoDatas = ValidarDatas(atividade);
                if (!validacaoDatas.IsValid)
                {
                    result.Errors.AddRange(validacaoDatas.Errors);
                    result.IsValid = false;
                }

                // Validar status e conclusão
                var validacaoStatus = await ValidarStatusConclusao(atividade);
                if (!validacaoStatus.IsValid)
                {
                    result.Errors.AddRange(validacaoStatus.Errors);
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro durante validação da atividade: {ex.Message}", LogLevel.Error);
                result.Errors.Add("Erro interno durante validação");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// Valida se o projeto existe e está ativo
        /// </summary>
        private async Task<ValidationResult> ValidarProjeto(int projetoId)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            var projeto = await _context.Projetos
                .Include(p => p.Status)
                .FirstOrDefaultAsync(p => p.Id == projetoId);

            if (projeto == null)
            {
                result.Errors.Add("Projeto selecionado não encontrado");
                result.IsValid = false;
            }
            else if (projeto.Status.Nome == "Cancelado")
            {
                result.Errors.Add("Não é possível atribuir atividades a projetos cancelados");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// Valida se o usuário existe e está ativo
        /// </summary>
        private async Task<ValidationResult> ValidarUsuario(int usuarioId)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                result.Errors.Add("Usuário selecionado não encontrado");
                result.IsValid = false;
            }
            else if (!usuario.Ativo)
            {
                result.Errors.Add("Não é possível atribuir atividades a usuários inativos");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// Valida consistência das datas
        /// </summary>
        private static ValidationResult ValidarDatas(AtividadeViewModel atividade)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (atividade.DataFimPrevista.HasValue && atividade.DataFimPrevista.Value < atividade.DataInicio)
            {
                result.Errors.Add("Data fim prevista não pode ser anterior à data de início");
                result.IsValid = false;
            }

            if (atividade.DataFimReal.HasValue && atividade.DataFimReal.Value < atividade.DataInicio)
            {
                result.Errors.Add("Data de conclusão não pode ser anterior à data de início");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// Valida regras de status e conclusão
        /// </summary>
        private async Task<ValidationResult> ValidarStatusConclusao(AtividadeViewModel atividade)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            var status = await _context.StatusAtividades
                .FirstOrDefaultAsync(s => s.Id == atividade.StatusId);

            if (status?.Nome == "Concluída" && !atividade.DataFimReal.HasValue)
            {
                result.Errors.Add("Data de conclusão é obrigatória para atividades concluídas");
                result.IsValid = false;
            }

            if (status?.Nome != "Concluída" && atividade.DataFimReal.HasValue)
            {
                result.Errors.Add("Data de conclusão só deve ser preenchida para atividades concluídas");
                result.IsValid = false;
            }

            return result;
        }

        private static AtividadeViewModel MapToViewModel(Atividade atividade)
        {
            return new AtividadeViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                Descricao = atividade.Descricao,
                ProjetoId = atividade.ProjetoId,
                ProjetoNome = atividade.Projeto?.Nome,
                TipoAtividadeId = atividade.TipoAtividadeId,
                TipoAtividadeNome = atividade.TipoAtividade?.Nome,
                StatusId = atividade.StatusId,
                StatusNome = atividade.Status?.Nome,
                StatusCor = atividade.Status?.Cor,
                UsuarioId = atividade.UsuarioId,
                UsuarioNome = atividade.Usuario?.Nome,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimPrevista,
                DataFimReal = atividade.DataFimReal,
                HorasEstimadas = atividade.HorasEstimadas,
                HorasReais = atividade.HorasReais,
                Prioridade = atividade.Prioridade,
                Observacoes = atividade.Observacoes,
                DataCriacao = atividade.DataCriacao,
                DataAtualizacao = atividade.DataAtualizacao
            };
        }

        private static void MapViewModelToEntity(AtividadeViewModel viewModel, Atividade entity)
        {
            entity.Nome = viewModel.Nome;
            entity.Descricao = viewModel.Descricao;
            entity.ProjetoId = viewModel.ProjetoId;
            entity.TipoAtividadeId = viewModel.TipoAtividadeId;
            entity.StatusId = viewModel.StatusId;
            entity.UsuarioId = viewModel.UsuarioId;
            entity.DataInicio = viewModel.DataInicio;
            entity.DataFimPrevista = viewModel.DataFimPrevista;
            entity.DataFimReal = viewModel.DataFimReal;
            entity.HorasEstimadas = viewModel.HorasEstimadas;
            entity.HorasReais = viewModel.HorasReais;
            entity.Prioridade = viewModel.Prioridade;
            entity.Observacoes = viewModel.Observacoes;
        }

        private bool AtividadeExists(int id)
        {
            try
            {
                return _context.Atividades.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                LogComContexto($"Erro ao verificar existência da atividade {id}: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // Classe auxiliar para validação
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
}