
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.Common;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Implementation
{
    /// <summary>
    /// Service para operações de escrita de projetos
    /// Implementa Single Responsibility Principle
    /// </summary>
    public class ProjetoCommandService : IProjetoCommandService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<ProjetoCommandService> _logger;
        private readonly IProjetoValidationService _validationService;
        private readonly IProjetoMapper _mapper;

        public ProjetoCommandService(
            ProGestaoContext context,
            ILogger<ProjetoCommandService> logger,
            IProjetoValidationService validationService,
            IProjetoMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<int>> CreateProjetoAsync(ProjetoViewModel projetoVm)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de projeto: {Nome}", projetoVm.Nome);

                var validation = await _validationService.ValidateCreateAsync(projetoVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para criação de projeto: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<int>.Failure("Dados inválidos", validation.Errors);
                }

                var projeto = _mapper.MapToEntity(projetoVm);
                projeto.DataCriacao = DateTime.Now;

                _context.Projetos.Add(projeto);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Projeto criado com sucesso. ID: {ProjetoId}, Nome: {Nome}",
                        projeto.Id, projeto.Nome);
                    return Result<int>.Success(projeto.Id, "Projeto criado com sucesso!");
                }

                return Result<int>.Failure("Erro ao salvar projeto no banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar projeto: {Nome}", projetoVm.Nome);
                return Result<int>.Failure("Erro interno ao criar projeto");
            }
        }

        public async Task<Result<bool>> UpdateProjetoAsync(ProjetoViewModel projetoVm)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de projeto: {ProjetoId}", projetoVm.Id);

                var validation = await _validationService.ValidateUpdateAsync(projetoVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para atualização de projeto: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Dados inválidos", validation.Errors);
                }

                var projeto = await _context.Projetos.FindAsync(projetoVm.Id);
                if (projeto == null)
                {
                    _logger.LogWarning("Projeto não encontrado para atualização: {ProjetoId}", projetoVm.Id);
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                var statusAnterior = projeto.StatusId;
                var responsavelAnterior = projeto.ResponsavelId;
                var dataOriginalCriacao = projeto.DataCriacao;

                _mapper.MapToEntityUpdate(projetoVm, projeto);
                projeto.DataCriacao = dataOriginalCriacao;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    if (statusAnterior != projetoVm.StatusId)
                    {
                        await LogMudancaStatusAsync(statusAnterior, projetoVm.StatusId, projetoVm.Id);
                    }

                    if (responsavelAnterior != projetoVm.ResponsavelId)
                    {
                        await LogMudancaResponsavelAsync(responsavelAnterior, projetoVm.ResponsavelId, projetoVm.Id);
                    }

                    _logger.LogInformation("Projeto atualizado com sucesso. ID: {ProjetoId}, Nome: {Nome}",
                        projeto.Id, projeto.Nome);
                    return Result<bool>.Success(true, "Projeto atualizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao salvar alterações no banco de dados");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao atualizar projeto: {ProjetoId}", projetoVm.Id);
                return Result<bool>.Failure("O projeto foi modificado por outro usuário. Recarregue a página e tente novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar projeto: {ProjetoId}", projetoVm.Id);
                return Result<bool>.Failure("Erro interno ao atualizar projeto");
            }
        }

        public async Task<Result<bool>> DeleteProjetoAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão de projeto: {ProjetoId}", id);

                var validation = await _validationService.ValidateDeleteAsync(id);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para exclusão de projeto: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Não é possível excluir o projeto", validation.Errors);
                }

                var projeto = await _context.Projetos
                    .Include(p => p.Atividades)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto não encontrado para exclusão: {ProjetoId}", id);
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                var nomeProjeto = projeto.Nome;
                var quantidadeAtividades = projeto.Atividades?.Count ?? 0;

                _context.Projetos.Remove(projeto);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Projeto excluído com sucesso. ID: {ProjetoId}, Nome: {Nome}, Atividades: {QtdAtividades}",
                        id, nomeProjeto, quantidadeAtividades);
                    return Result<bool>.Success(true, "Projeto excluído com sucesso!");
                }

                return Result<bool>.Failure("Erro ao excluir projeto do banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir projeto: {ProjetoId}", id);
                return Result<bool>.Failure("Erro interno ao excluir projeto");
            }
        }

        public async Task<Result<bool>> AlterarStatusProjetoAsync(int projetoId, int novoStatusId)
        {
            try
            {
                _logger.LogInformation("Alterando status do projeto: {ProjetoId} para status: {StatusId}",
                    projetoId, novoStatusId);

                var validation = await _validationService.ValidateStatusChangeAsync(projetoId, novoStatusId);
                if (!validation.IsValid)
                {
                    return Result<bool>.Failure("Mudança de status inválida", validation.Errors);
                }

                var projeto = await _context.Projetos.FindAsync(projetoId);
                if (projeto == null)
                {
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                var statusAnterior = projeto.StatusId;
                projeto.StatusId = novoStatusId;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await LogMudancaStatusAsync(statusAnterior, novoStatusId, projetoId);
                    return Result<bool>.Success(true, "Status do projeto atualizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao atualizar status do projeto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status do projeto: {ProjetoId}", projetoId);
                return Result<bool>.Failure("Erro interno ao alterar status");
            }
        }

        public async Task<Result<bool>> AtribuirResponsavelAsync(int projetoId, int? responsavelId)
        {
            try
            {
                var validation = await _validationService.ValidateResponsavelChangeAsync(projetoId, responsavelId);
                if (!validation.IsValid)
                {
                    return Result<bool>.Failure("Atribuição de responsável inválida", validation.Errors);
                }

                var projeto = await _context.Projetos.FindAsync(projetoId);
                if (projeto == null)
                {
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                var responsavelAnterior = projeto.ResponsavelId;
                projeto.ResponsavelId = responsavelId;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await LogMudancaResponsavelAsync(responsavelAnterior, responsavelId, projetoId);
                    return Result<bool>.Success(true, "Responsável do projeto atualizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao atribuir responsável");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir responsável ao projeto: {ProjetoId}", projetoId);
                return Result<bool>.Failure("Erro interno ao atribuir responsável");
            }
        }

        public async Task<Result<bool>> FinalizarProjetoAsync(int projetoId, DateTime? dataFim = null)
        {
            try
            {
                var projeto = await _context.Projetos.FindAsync(projetoId);
                if (projeto == null)
                {
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                projeto.DataFimReal = dataFim ?? DateTime.Now;

                var statusConcluido = await _context.StatusProjetos
                    .FirstOrDefaultAsync(s => s.Nome == "Concluído");

                if (statusConcluido != null)
                {
                    projeto.StatusId = statusConcluido.Id;
                }

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Projeto finalizado com sucesso: {ProjetoId}", projetoId);
                    return Result<bool>.Success(true, "Projeto finalizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao finalizar projeto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar projeto: {ProjetoId}", projetoId);
                return Result<bool>.Failure("Erro interno ao finalizar projeto");
            }
        }

        public async Task<Result<bool>> CancelarProjetoAsync(int projetoId, string? motivoCancelamento = null)
        {
            try
            {
                var projeto = await _context.Projetos.FindAsync(projetoId);
                if (projeto == null)
                {
                    return Result<bool>.Failure("Projeto não encontrado");
                }

                var statusCancelado = await _context.StatusProjetos
                    .FirstOrDefaultAsync(s => s.Nome == "Cancelado");

                if (statusCancelado != null)
                {
                    projeto.StatusId = statusCancelado.Id;
                }

                if (!string.IsNullOrWhiteSpace(motivoCancelamento))
                {
                    projeto.Observacoes = string.IsNullOrWhiteSpace(projeto.Observacoes)
                        ? $"Cancelado: {motivoCancelamento}"
                        : $"{projeto.Observacoes}\n\nCancelado: {motivoCancelamento}";
                }

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Projeto cancelado com sucesso: {ProjetoId}. Motivo: {Motivo}",
                        projetoId, motivoCancelamento);
                    return Result<bool>.Success(true, "Projeto cancelado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao cancelar projeto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar projeto: {ProjetoId}", projetoId);
                return Result<bool>.Failure("Erro interno ao cancelar projeto");
            }
        }

        private async Task LogMudancaStatusAsync(int statusAnteriorId, int statusNovoId, int projetoId)
        {
            try
            {
                var statusAnterior = await _context.StatusProjetos
                    .Where(s => s.Id == statusAnteriorId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                var statusNovo = await _context.StatusProjetos
                    .Where(s => s.Id == statusNovoId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Status do projeto {ProjetoId} alterado de '{StatusAnterior}' para '{StatusNovo}'",
                    projetoId, statusAnterior, statusNovo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao logar mudança de status para projeto: {ProjetoId}", projetoId);
            }
        }

        private async Task LogMudancaResponsavelAsync(int? responsavelAnteriorId, int? responsavelNovoId, int projetoId)
        {
            try
            {
                var responsavelAnterior = responsavelAnteriorId.HasValue
                    ? await _context.Usuarios
                        .Where(u => u.Id == responsavelAnteriorId.Value)
                        .Select(u => u.Nome)
                        .FirstOrDefaultAsync()
                    : "Nenhum";

                var responsavelNovo = responsavelNovoId.HasValue
                    ? await _context.Usuarios
                        .Where(u => u.Id == responsavelNovoId.Value)
                        .Select(u => u.Nome)
                        .FirstOrDefaultAsync()
                    : "Nenhum";

                _logger.LogInformation("Responsável do projeto {ProjetoId} alterado de '{ResponsavelAnterior}' para '{ResponsavelNovo}'",
                    projetoId, responsavelAnterior, responsavelNovo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao logar mudança de responsável para projeto: {ProjetoId}", projetoId);
            }
        }
    }
}

