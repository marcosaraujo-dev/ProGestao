using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Atividades
{
    /// <summary>
    /// Service para validação de atividades
    /// Implementa Single Responsibility Principle
    /// </summary>
    public class AtividadeValidationService : IAtividadeValidationService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<AtividadeValidationService> _logger;

        public AtividadeValidationService(
            ProGestaoContext context,
            ILogger<AtividadeValidationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidationResult> ValidateCreateAsync(AtividadeViewModel atividade)
        {
            var result = new ValidationResult { IsValid = true };

            await ValidateBasicFields(atividade, result);
            await ValidateBusinessRules(atividade, result);
            await ValidateForeignKeys(atividade, result);

            return result;
        }

        public async Task<ValidationResult> ValidateUpdateAsync(AtividadeViewModel atividade)
        {
            var result = new ValidationResult { IsValid = true };

            if (atividade.Id <= 0)
            {
                result.AddError("ID da atividade é obrigatório para atualização");
                return result;
            }

            var existe = await _context.Atividades.AnyAsync(a => a.Id == atividade.Id);
            if (!existe)
            {
                result.AddError("Atividade não encontrada");
                return result;
            }

            await ValidateBasicFields(atividade, result);
            await ValidateBusinessRules(atividade, result);
            await ValidateForeignKeys(atividade, result);

            return result;
        }

        public async Task<ValidationResult> ValidateDeleteAsync(int atividadeId)
        {
            var result = new ValidationResult { IsValid = true };

            if (atividadeId <= 0)
            {
                result.AddError("ID da atividade inválido");
                return result;
            }

            var atividade = await _context.Atividades
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == atividadeId);

            if (atividade == null)
            {
                result.AddError("Atividade não encontrada");
                return result;
            }

            if (atividade.Status?.Nome == "Em Andamento")
            {
                result.AddError("Não é possível excluir atividade em andamento");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateStatusChangeAsync(int atividadeId, int novoStatusId)
        {
            var result = new ValidationResult { IsValid = true };

            var atividade = await _context.Atividades
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == atividadeId);

            if (atividade == null)
            {
                result.AddError("Atividade não encontrada");
                return result;
            }

            var novoStatus = await _context.StatusAtividades
                .FirstOrDefaultAsync(s => s.Id == novoStatusId);

            if (novoStatus == null)
            {
                result.AddError("Status inválido");
                return result;
            }

            await ValidateStatusTransition(atividade.Status?.Nome, novoStatus.Nome, result);

            return result;
        }

        private async Task ValidateBasicFields(AtividadeViewModel atividade, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(atividade.Nome))
            {
                result.AddError("Nome da atividade é obrigatório");
            }
            else if (atividade.Nome.Length > 200)
            {
                result.AddError("Nome da atividade deve ter no máximo 200 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(atividade.Descricao) && atividade.Descricao.Length > 1000)
            {
                result.AddError("Descrição deve ter no máximo 1000 caracteres");
            }

            if (atividade.DataInicio == default)
            {
                result.AddError("Data de início é obrigatória");
            }

            if (atividade.DataFimPrevista.HasValue && atividade.DataFimPrevista < atividade.DataInicio)
            {
                result.AddError("Data fim prevista não pode ser anterior à data de início");
            }

            if (atividade.DataFimReal.HasValue && atividade.DataFimReal < atividade.DataInicio)
            {
                result.AddError("Data fim real não pode ser anterior à data de início");
            }

            if (atividade.HorasEstimadas.HasValue && atividade.HorasEstimadas < 0)
            {
                result.AddError("Horas estimadas não pode ser negativa");
            }

            if (atividade.HorasReais.HasValue && atividade.HorasReais < 0)
            {
                result.AddError("Horas reais não pode ser negativa");
            }

            if (atividade.Prioridade < 1 || atividade.Prioridade > 4)
            {
                result.AddError("Prioridade deve estar entre 1 e 4");
            }

            await Task.CompletedTask;
        }

        private async Task ValidateBusinessRules(AtividadeViewModel atividade, ValidationResult result)
        {
            if (atividade.TipoAtividadeId <= 0)
            {
                result.AddError("Tipo de atividade é obrigatório");
            }

            if (atividade.StatusId <= 0)
            {
                result.AddError("Status é obrigatório");
            }

            if (atividade.UsuarioId <= 0)
            {
                result.AddError("Usuário responsável é obrigatório");
            }

            if (atividade.ProjetoId.HasValue)
            {
                var projetoAtivo = await _context.Projetos
                    .Include(p => p.Status)
                    .Where(p => p.Id == atividade.ProjetoId.Value)
                    .Select(p => p.Status.Nome)
                    .FirstOrDefaultAsync();

                if (projetoAtivo == "Cancelado")
                {
                    result.AddError("Não é possível criar atividade para projeto cancelado");
                }
            }

            var usuarioAtivo = await _context.Usuarios
                .Where(u => u.Id == atividade.UsuarioId)
                .Select(u => u.Ativo)
                .FirstOrDefaultAsync();

            if (!usuarioAtivo)
            {
                result.AddError("Usuário selecionado não está ativo");
            }
        }

        private async Task ValidateForeignKeys(AtividadeViewModel atividade, ValidationResult result)
        {
            if (atividade.ProjetoId.HasValue)
            {
                var projetoExiste = await _context.Projetos.AnyAsync(p => p.Id == atividade.ProjetoId.Value);
                if (!projetoExiste)
                {
                    result.AddError("Projeto selecionado não existe");
                }
            }

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == atividade.UsuarioId);
            if (!usuarioExiste)
            {
                result.AddError("Usuário selecionado não existe");
            }

            var tipoExiste = await _context.TiposAtividade.AnyAsync(t => t.Id == atividade.TipoAtividadeId);
            if (!tipoExiste)
            {
                result.AddError("Tipo de atividade selecionado não existe");
            }

            var statusExiste = await _context.StatusAtividades.AnyAsync(s => s.Id == atividade.StatusId);
            if (!statusExiste)
            {
                result.AddError("Status selecionado não existe");
            }
        }

        private async Task ValidateStatusTransition(string? statusAtual, string statusNovo, ValidationResult result)
        {
            if (string.IsNullOrEmpty(statusAtual))
                return;

            var transitionsInvalidas = new Dictionary<string, HashSet<string>>
            {
                ["Concluída"] = new HashSet<string> { "Pendente", "Em Andamento" },
                ["Cancelada"] = new HashSet<string> { "Pendente", "Em Andamento", "Concluída" }
            };

            if (transitionsInvalidas.ContainsKey(statusAtual) &&
                transitionsInvalidas[statusAtual].Contains(statusNovo))
            {
                result.AddError($"Não é possível alterar status de '{statusAtual}' para '{statusNovo}'");
            }

            await Task.CompletedTask;
        }
    }



}
