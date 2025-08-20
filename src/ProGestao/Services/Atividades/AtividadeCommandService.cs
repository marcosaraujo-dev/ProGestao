using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Atividades
{
    /// <summary>
    /// Service para operações de escrita de atividades
    /// </summary>
    public class AtividadeCommandService : IAtividadeCommandService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<AtividadeCommandService> _logger;
        private readonly IAtividadeValidationService _validationService;
        private readonly IAtividadeMapper _mapper; 

        public AtividadeCommandService(
            ProGestaoContext context,
            ILogger<AtividadeCommandService> logger,
            IAtividadeValidationService validationService,
            IAtividadeMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<int>> CreateAtividadeAsync(AtividadeViewModel atividadeVm)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de atividade: {Nome}", atividadeVm.Nome);

                var validation = await _validationService.ValidateCreateAsync(atividadeVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para criação de atividade: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<int>.Failure("Dados inválidos", validation.Errors);
                }

                var atividade = _mapper.MapToEntity(atividadeVm);
                atividade.DataCriacao = DateTime.Now;
                atividade.DataAtualizacao = DateTime.Now;

                _context.Atividades.Add(atividade);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Atividade criada com sucesso. ID: {AtividadeId}, Nome: {Nome}",
                        atividade.Id, atividade.Nome);
                    return Result<int>.Success(atividade.Id, "Atividade criada com sucesso!");
                }

                return Result<int>.Failure("Erro ao salvar atividade no banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar atividade: {Nome}", atividadeVm.Nome);
                return Result<int>.Failure("Erro interno ao criar atividade");
            }
        }

        public async Task<Result<bool>> UpdateAtividadeAsync(AtividadeViewModel atividadeVm)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de atividade: {AtividadeId}", atividadeVm.Id);

                var validation = await _validationService.ValidateUpdateAsync(atividadeVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para atualização de atividade: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Dados inválidos", validation.Errors);
                }

                var atividade = await _context.Atividades.FindAsync(atividadeVm.Id);
                if (atividade == null)
                {
                    _logger.LogWarning("Atividade não encontrada para atualização: {AtividadeId}", atividadeVm.Id);
                    return Result<bool>.Failure("Atividade não encontrada");
                }

                var statusAnterior = atividade.StatusId;
                var dataOriginalCriacao = atividade.DataCriacao;

                _mapper.MapToEntityUpdate(atividadeVm, atividade);
                atividade.DataCriacao = dataOriginalCriacao;
                atividade.DataAtualizacao = DateTime.Now;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    if (statusAnterior != atividadeVm.StatusId)
                    {
                        await LogMudancaStatusAsync(statusAnterior, atividadeVm.StatusId, atividadeVm.Id);
                    }

                    _logger.LogInformation("Atividade atualizada com sucesso. ID: {AtividadeId}, Nome: {Nome}",
                        atividade.Id, atividade.Nome);
                    return Result<bool>.Success(true, "Atividade atualizada com sucesso!");
                }

                return Result<bool>.Failure("Erro ao salvar alterações no banco de dados");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao atualizar atividade: {AtividadeId}", atividadeVm.Id);
                return Result<bool>.Failure("A atividade foi modificada por outro usuário. Recarregue a página e tente novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar atividade: {AtividadeId}", atividadeVm.Id);
                return Result<bool>.Failure("Erro interno ao atualizar atividade");
            }
        }

        public async Task<Result<bool>> DeleteAtividadeAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão de atividade: {AtividadeId}", id);

                var validation = await _validationService.ValidateDeleteAsync(id);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para exclusão de atividade: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Não é possível excluir a atividade", validation.Errors);
                }

                var atividade = await _context.Atividades.FindAsync(id);
                if (atividade == null)
                {
                    _logger.LogWarning("Atividade não encontrada para exclusão: {AtividadeId}", id);
                    return Result<bool>.Failure("Atividade não encontrada");
                }

                var nomeAtividade = atividade.Nome;
                _context.Atividades.Remove(atividade);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Atividade excluída com sucesso. ID: {AtividadeId}, Nome: {Nome}",
                        id, nomeAtividade);
                    return Result<bool>.Success(true, "Atividade excluída com sucesso!");
                }

                return Result<bool>.Failure("Erro ao excluir atividade do banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir atividade: {AtividadeId}", id);
                return Result<bool>.Failure("Erro interno ao excluir atividade");
            }
        }

        public async Task<Result<bool>> UpdateStatusAsync(int atividadeId, int novoStatusId)
        {
            try
            {
                _logger.LogInformation("Atualizando status da atividade: {AtividadeId} para status: {StatusId}",
                    atividadeId, novoStatusId);

                var validation = await _validationService.ValidateStatusChangeAsync(atividadeId, novoStatusId);
                if (!validation.IsValid)
                {
                    return Result<bool>.Failure("Mudança de status inválida", validation.Errors);
                }

                var atividade = await _context.Atividades.FindAsync(atividadeId);
                if (atividade == null)
                {
                    return Result<bool>.Failure("Atividade não encontrada");
                }

                var statusAnterior = atividade.StatusId;
                atividade.StatusId = novoStatusId;
                atividade.DataAtualizacao = DateTime.Now;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await LogMudancaStatusAsync(statusAnterior, novoStatusId, atividadeId);
                    return Result<bool>.Success(true, "Status atualizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao atualizar status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status da atividade: {AtividadeId}", atividadeId);
                return Result<bool>.Failure("Erro interno ao atualizar status");
            }
        }

        public async Task<Result<bool>> FinalizarAtividadeAsync(int atividadeId, DateTime? dataFim = null)
        {
            try
            {
                var atividade = await _context.Atividades.FindAsync(atividadeId);
                if (atividade == null)
                {
                    return Result<bool>.Failure("Atividade não encontrada");
                }

                atividade.DataFimReal = dataFim ?? DateTime.Now;

                var statusConcluida = await _context.StatusAtividades
                    .FirstOrDefaultAsync(s => s.Nome == "Concluída");

                if (statusConcluida != null)
                {
                    atividade.StatusId = statusConcluida.Id;
                }

                atividade.DataAtualizacao = DateTime.Now;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Atividade finalizada com sucesso: {AtividadeId}", atividadeId);
                    return Result<bool>.Success(true, "Atividade finalizada com sucesso!");
                }

                return Result<bool>.Failure("Erro ao finalizar atividade");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar atividade: {AtividadeId}", atividadeId);
                return Result<bool>.Failure("Erro interno ao finalizar atividade");
            }
        }

        private async Task LogMudancaStatusAsync(int statusAnteriorId, int statusNovoId, int atividadeId)
        {
            try
            {
                var statusAnterior = await _context.StatusAtividades
                    .Where(s => s.Id == statusAnteriorId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                var statusNovo = await _context.StatusAtividades
                    .Where(s => s.Id == statusNovoId)
                    .Select(s => s.Nome)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Status da atividade {AtividadeId} alterado de '{StatusAnterior}' para '{StatusNovo}'",
                    atividadeId, statusAnterior, statusNovo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao logar mudança de status para atividade: {AtividadeId}", atividadeId);
            }
        }
    }
}
