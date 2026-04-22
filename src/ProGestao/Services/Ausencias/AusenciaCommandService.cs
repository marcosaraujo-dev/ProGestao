using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Ausencias
{
    /// <summary>
    /// Service para operações de escrita de ausências
    /// Usa Result Pattern para retorno padronizado
    /// </summary>
    public class AusenciaCommandService : IAusenciaCommandService
    {
        private readonly ProGestaoContext _context;
        private readonly IAusenciaValidationService _validationService;
        private readonly ILogger<AusenciaCommandService> _logger;

        public AusenciaCommandService(
            ProGestaoContext context,
            IAusenciaValidationService validationService,
            ILogger<AusenciaCommandService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<int>> CreateAsync(AusenciaViewModel ausenciaVm)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando criação de ausência para usuário {UsuarioId}", ausenciaVm.UsuarioId);

                var validation = await _validationService.ValidateCreateAsync(ausenciaVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para criação de ausência: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<int>.Failure("Dados inválidos", validation.Errors);
                }

                var ausencia = new Ausencia
                {
                    UsuarioId = ausenciaVm.UsuarioId,
                    TipoAusenciaId = ausenciaVm.TipoAusenciaId,
                    DataInicio = ausenciaVm.DataInicio,
                    DataFim = ausenciaVm.DataFim,
                    Observacao = ausenciaVm.Observacao?.Trim(),
                    DataCriacao = DateTime.Now,
                    Ativo = true
                };

                _context.Ausencias.Add(ausencia);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation(
                        "Ausência criada com sucesso. ID: {AusenciaId}, Usuário: {UsuarioId}",
                        ausencia.Id, ausencia.UsuarioId);
                    return Result<int>.Success(ausencia.Id, "Ausência criada com sucesso!");
                }

                return Result<int>.Failure("Erro ao salvar ausência no banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ausência para usuário {UsuarioId}", ausenciaVm.UsuarioId);
                return Result<int>.Failure("Erro interno ao criar ausência");
            }
        }

        public async Task<Result<bool>> UpdateAsync(AusenciaViewModel ausenciaVm)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de ausência: {AusenciaId}", ausenciaVm.Id);

                var validation = await _validationService.ValidateUpdateAsync(ausenciaVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para atualização de ausência: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Dados inválidos", validation.Errors);
                }

                var ausencia = await _context.Ausencias.FindAsync(ausenciaVm.Id);
                if (ausencia == null)
                {
                    _logger.LogWarning("Ausência não encontrada para atualização: {AusenciaId}", ausenciaVm.Id);
                    return Result<bool>.Failure("Ausência não encontrada");
                }

                ausencia.UsuarioId = ausenciaVm.UsuarioId;
                ausencia.TipoAusenciaId = ausenciaVm.TipoAusenciaId;
                ausencia.DataInicio = ausenciaVm.DataInicio;
                ausencia.DataFim = ausenciaVm.DataFim;
                ausencia.Observacao = ausenciaVm.Observacao?.Trim();

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation(
                        "Ausência atualizada com sucesso. ID: {AusenciaId}, Usuário: {UsuarioId}",
                        ausencia.Id, ausencia.UsuarioId);
                    return Result<bool>.Success(true, "Ausência atualizada com sucesso!");
                }

                return Result<bool>.Failure("Erro ao salvar alterações no banco de dados");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao atualizar ausência: {AusenciaId}", ausenciaVm.Id);
                return Result<bool>.Failure(
                    "A ausência foi modificada por outro usuário. Recarregue a página e tente novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar ausência: {AusenciaId}", ausenciaVm.Id);
                return Result<bool>.Failure("Erro interno ao atualizar ausência");
            }
        }

        public async Task<Result<bool>> DesativarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando desativação de ausência: {AusenciaId}", id);

                var ausencia = await _context.Ausencias.FindAsync(id);
                if (ausencia == null)
                {
                    _logger.LogWarning("Ausência não encontrada para desativação: {AusenciaId}", id);
                    return Result<bool>.Failure("Ausência não encontrada");
                }

                ausencia.Ativo = false;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation(
                        "Ausência desativada com sucesso. ID: {AusenciaId}", ausencia.Id);
                    return Result<bool>.Success(true, "Ausência desativada com sucesso!");
                }

                return Result<bool>.Failure("Erro ao desativar ausência");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar ausência: {AusenciaId}", id);
                return Result<bool>.Failure("Erro interno ao desativar ausência");
            }
        }
    }
}
