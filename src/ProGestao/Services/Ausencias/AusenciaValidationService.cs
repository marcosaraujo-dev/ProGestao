using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Ausencias
{
    /// <summary>
    /// Service para validação de ausências
    /// Valida datas (DataInicio &lt;= DataFim) e sobreposição de períodos
    /// </summary>
    public class AusenciaValidationService : IAusenciaValidationService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<AusenciaValidationService> _logger;

        public AusenciaValidationService(
            ProGestaoContext context,
            ILogger<AusenciaValidationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidationResult> ValidateCreateAsync(AusenciaViewModel ausencia)
        {
            var result = ValidateDatas(ausencia.DataInicio, ausencia.DataFim);
            if (!result.IsValid)
                return result;

            if (ausencia.UsuarioId <= 0)
            {
                result.AddError("Usuário é obrigatório");
                return result;
            }

            if (ausencia.TipoAusenciaId <= 0)
            {
                result.AddError("Tipo de ausência é obrigatório");
                return result;
            }

            var sobreposicao = await ValidateSobreposicaoAsync(
                ausencia.UsuarioId, ausencia.DataInicio, ausencia.DataFim);

            if (!sobreposicao.IsValid)
                return sobreposicao;

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateUpdateAsync(AusenciaViewModel ausencia)
        {
            var result = ValidateDatas(ausencia.DataInicio, ausencia.DataFim);
            if (!result.IsValid)
                return result;

            if (ausencia.Id <= 0)
            {
                result.AddError("ID da ausência é obrigatório para atualização");
                return result;
            }

            if (ausencia.UsuarioId <= 0)
            {
                result.AddError("Usuário é obrigatório");
                return result;
            }

            if (ausencia.TipoAusenciaId <= 0)
            {
                result.AddError("Tipo de ausência é obrigatório");
                return result;
            }

            var sobreposicao = await ValidateSobreposicaoAsync(
                ausencia.UsuarioId, ausencia.DataInicio, ausencia.DataFim, ausencia.Id);

            if (!sobreposicao.IsValid)
                return sobreposicao;

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateSobreposicaoAsync(
            int usuarioId, DateTime dataInicio, DateTime dataFim, int? ausenciaIdExcluir = null)
        {
            try
            {
                _logger.LogInformation(
                    "Verificando sobreposição de ausências para usuário {UsuarioId} no período {DataInicio} a {DataFim}",
                    usuarioId, dataInicio, dataFim);

                // Dois períodos [A1,A2] e [B1,B2] se sobrepõem quando A1 <= B2 AND B1 <= A2
                var query = _context.Ausencias
                    .Where(a => a.UsuarioId == usuarioId
                        && a.Ativo
                        && a.DataInicio <= dataFim
                        && dataInicio <= a.DataFim);

                if (ausenciaIdExcluir.HasValue)
                {
                    query = query.Where(a => a.Id != ausenciaIdExcluir.Value);
                }

                var existeSobreposicao = await query
                    .AsNoTracking()
                    .AnyAsync();

                if (existeSobreposicao)
                {
                    _logger.LogWarning(
                        "Sobreposição detectada para usuário {UsuarioId} no período {DataInicio} a {DataFim}",
                        usuarioId, dataInicio, dataFim);

                    return ValidationResult.Failure(
                        "Já existe uma ausência ativa para este usuário no período informado");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao verificar sobreposição de ausências para usuário {UsuarioId}", usuarioId);
                return ValidationResult.Failure("Erro ao verificar sobreposição de ausências");
            }
        }

        private static ValidationResult ValidateDatas(DateTime dataInicio, DateTime dataFim)
        {
            if (dataInicio > dataFim)
            {
                return ValidationResult.Failure("Data de início deve ser menor ou igual à data de fim");
            }

            return ValidationResult.Success();
        }
    }
}
