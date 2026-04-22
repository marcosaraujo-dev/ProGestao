using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Services.TiposAusencia
{
    /// <summary>
    /// Service para operações de escrita de tipos de ausência
    /// </summary>
    public partial class TipoAusenciaCommandService : ITipoAusenciaCommandService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<TipoAusenciaCommandService> _logger;

        [GeneratedRegex(@"^#[0-9A-Fa-f]{6}$")]
        private static partial Regex HexColorRegex();

        public TipoAusenciaCommandService(
            ProGestaoContext context,
            ILogger<TipoAusenciaCommandService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<int>> CreateAsync(TipoAusenciaViewModel tipoAusenciaVm)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de tipo de ausência: {Nome}", tipoAusenciaVm.Nome);

                var validation = Validate(tipoAusenciaVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para criação de tipo de ausência: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<int>.Failure("Dados inválidos", validation.Errors);
                }

                var tipoAusencia = new TipoAusencia
                {
                    Nome = tipoAusenciaVm.Nome.Trim(),
                    Cor = tipoAusenciaVm.Cor,
                    Descricao = tipoAusenciaVm.Descricao?.Trim(),
                    Ativo = true
                };

                _context.TiposAusencia.Add(tipoAusencia);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Tipo de ausência criado com sucesso. ID: {TipoAusenciaId}, Nome: {Nome}",
                        tipoAusencia.Id, tipoAusencia.Nome);
                    return Result<int>.Success(tipoAusencia.Id, "Tipo de ausência criado com sucesso!");
                }

                return Result<int>.Failure("Erro ao salvar tipo de ausência no banco de dados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tipo de ausência: {Nome}", tipoAusenciaVm.Nome);
                return Result<int>.Failure("Erro interno ao criar tipo de ausência");
            }
        }

        public async Task<Result<bool>> UpdateAsync(TipoAusenciaViewModel tipoAusenciaVm)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de tipo de ausência: {TipoAusenciaId}", tipoAusenciaVm.Id);

                var validation = Validate(tipoAusenciaVm);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou para atualização de tipo de ausência: {Errors}",
                        string.Join(", ", validation.Errors));
                    return Result<bool>.Failure("Dados inválidos", validation.Errors);
                }

                var tipoAusencia = await _context.TiposAusencia.FindAsync(tipoAusenciaVm.Id);
                if (tipoAusencia == null)
                {
                    _logger.LogWarning("Tipo de ausência não encontrado para atualização: {TipoAusenciaId}", tipoAusenciaVm.Id);
                    return Result<bool>.Failure("Tipo de ausência não encontrado");
                }

                tipoAusencia.Nome = tipoAusenciaVm.Nome.Trim();
                tipoAusencia.Cor = tipoAusenciaVm.Cor;
                tipoAusencia.Descricao = tipoAusenciaVm.Descricao?.Trim();

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Tipo de ausência atualizado com sucesso. ID: {TipoAusenciaId}, Nome: {Nome}",
                        tipoAusencia.Id, tipoAusencia.Nome);
                    return Result<bool>.Success(true, "Tipo de ausência atualizado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao salvar alterações no banco de dados");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao atualizar tipo de ausência: {TipoAusenciaId}", tipoAusenciaVm.Id);
                return Result<bool>.Failure("O tipo de ausência foi modificado por outro usuário. Recarregue a página e tente novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tipo de ausência: {TipoAusenciaId}", tipoAusenciaVm.Id);
                return Result<bool>.Failure("Erro interno ao atualizar tipo de ausência");
            }
        }

        public async Task<Result<bool>> DesativarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando desativação de tipo de ausência: {TipoAusenciaId}", id);

                var tipoAusencia = await _context.TiposAusencia.FindAsync(id);
                if (tipoAusencia == null)
                {
                    _logger.LogWarning("Tipo de ausência não encontrado para desativação: {TipoAusenciaId}", id);
                    return Result<bool>.Failure("Tipo de ausência não encontrado");
                }

                tipoAusencia.Ativo = false;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Tipo de ausência desativado com sucesso. ID: {TipoAusenciaId}, Nome: {Nome}",
                        tipoAusencia.Id, tipoAusencia.Nome);
                    return Result<bool>.Success(true, "Tipo de ausência desativado com sucesso!");
                }

                return Result<bool>.Failure("Erro ao desativar tipo de ausência");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar tipo de ausência: {TipoAusenciaId}", id);
                return Result<bool>.Failure("Erro interno ao desativar tipo de ausência");
            }
        }

        private static ValidationResult Validate(TipoAusenciaViewModel tipoAusencia)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(tipoAusencia.Nome))
            {
                result.AddError("Nome é obrigatório");
            }
            else if (tipoAusencia.Nome.Length > 50)
            {
                result.AddError("Nome deve ter no máximo 50 caracteres");
            }

            if (string.IsNullOrWhiteSpace(tipoAusencia.Cor))
            {
                result.AddError("Cor é obrigatória");
            }
            else if (!HexColorRegex().IsMatch(tipoAusencia.Cor))
            {
                result.AddError("Cor deve estar no formato hexadecimal (#XXXXXX)");
            }

            if (tipoAusencia.Descricao != null && tipoAusencia.Descricao.Length > 200)
            {
                result.AddError("Descrição deve ter no máximo 200 caracteres");
            }

            return result;
        }
    }
}
