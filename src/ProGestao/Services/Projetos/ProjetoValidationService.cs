
using Microsoft.EntityFrameworkCore;
using ProGestao.Common;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Implementation
{
    // <summary>
    /// Service para validação de projetos
    /// </summary>
    public class ProjetoValidationService : IProjetoValidationService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<ProjetoValidationService> _logger;

        public ProjetoValidationService(
            ProGestaoContext context,
            ILogger<ProjetoValidationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidationResult> ValidateCreateAsync(ProjetoViewModel projeto)
        {
            var result = new ValidationResult { IsValid = true };

            await ValidateBasicFields(projeto, result);
            await ValidateBusinessRules(projeto, result);
            await ValidateForeignKeys(projeto, result);
            await ValidateUniqueConstraints(projeto, result);

            return result;
        }

        public async Task<ValidationResult> ValidateUpdateAsync(ProjetoViewModel projeto)
        {
            var result = new ValidationResult { IsValid = true };

            if (projeto.Id <= 0)
            {
                result.AddError("ID do projeto é obrigatório para atualização");
                return result;
            }

            var existe = await _context.Projetos.AnyAsync(p => p.Id == projeto.Id);
            if (!existe)
            {
                result.AddError("Projeto não encontrado");
                return result;
            }

            await ValidateBasicFields(projeto, result);
            await ValidateBusinessRules(projeto, result);
            await ValidateForeignKeys(projeto, result);
            await ValidateUniqueConstraints(projeto, result, projeto.Id);

            return result;
        }

        public async Task<ValidationResult> ValidateDeleteAsync(int projetoId)
        {
            var result = new ValidationResult { IsValid = true };

            if (projetoId <= 0)
            {
                result.AddError("ID do projeto inválido");
                return result;
            }

            var projeto = await _context.Projetos
                .Include(p => p.Status)
                .Include(p => p.Atividades)
                .FirstOrDefaultAsync(p => p.Id == projetoId);

            if (projeto == null)
            {
                result.AddError("Projeto não encontrado");
                return result;
            }

            if (projeto.Status?.Nome == "Em Andamento")
            {
                result.AddError("Não é possível excluir projeto em andamento");
            }

            if (projeto.Atividades?.Any(a => a.Status?.Nome == "Em Andamento") == true)
            {
                result.AddError("Não é possível excluir projeto com atividades em andamento");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateStatusChangeAsync(int projetoId, int novoStatusId)
        {
            var result = new ValidationResult { IsValid = true };

            var projeto = await _context.Projetos
                .Include(p => p.Status)
                .Include(p => p.Atividades)
                    .ThenInclude(a => a.Status)
                .FirstOrDefaultAsync(p => p.Id == projetoId);

            if (projeto == null)
            {
                result.AddError("Projeto não encontrado");
                return result;
            }

            var novoStatus = await _context.StatusProjetos
                .FirstOrDefaultAsync(s => s.Id == novoStatusId);

            if (novoStatus == null)
            {
                result.AddError("Status inválido");
                return result;
            }

            await ValidateStatusTransition(projeto.Status?.Nome, novoStatus.Nome, projeto, result);

            return result;
        }

        public async Task<ValidationResult> ValidateResponsavelChangeAsync(int projetoId, int? novoResponsavelId)
        {
            var result = new ValidationResult { IsValid = true };

            var projeto = await _context.Projetos.FindAsync(projetoId);
            if (projeto == null)
            {
                result.AddError("Projeto não encontrado");
                return result;
            }

            if (novoResponsavelId.HasValue)
            {
                var responsavel = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == novoResponsavelId.Value);

                if (responsavel == null)
                {
                    result.AddError("Responsável selecionado não existe");
                    return result;
                }

                if (!responsavel.Ativo)
                {
                    result.AddError("Responsável selecionado não está ativo");
                }
            }

            return result;
        }

        private async Task ValidateBasicFields(ProjetoViewModel projeto, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(projeto.Nome))
            {
                result.AddError("Nome do projeto é obrigatório");
            }
            else if (projeto.Nome.Length > 150)
            {
                result.AddError("Nome do projeto deve ter no máximo 150 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(projeto.Descricao) && projeto.Descricao.Length > 1000)
            {
                result.AddError("Descrição deve ter no máximo 1000 caracteres");
            }

            if (projeto.DataInicio == default)
            {
                result.AddError("Data de início é obrigatória");
            }

            if (projeto.DataFimPrevista.HasValue && projeto.DataFimPrevista < projeto.DataInicio)
            {
                result.AddError("Data fim prevista não pode ser anterior à data de início");
            }

            if (projeto.DataFimReal.HasValue && projeto.DataFimReal < projeto.DataInicio)
            {
                result.AddError("Data fim real não pode ser anterior à data de início");
            }

            if (!string.IsNullOrWhiteSpace(projeto.LinkProjeto))
            {
                if (!Uri.TryCreate(projeto.LinkProjeto, UriKind.Absolute, out _))
                {
                    result.AddError("Link do projeto deve ser uma URL válida");
                }
            }

            await Task.CompletedTask;
        }

        private async Task ValidateBusinessRules(ProjetoViewModel projeto, ValidationResult result)
        {
            if (projeto.StatusId <= 0)
            {
                result.AddError("Status é obrigatório");
            }

            if (projeto.ResponsavelId.HasValue && projeto.ResponsavelId.Value <= 0)
            {
                result.AddError("Responsável inválido");
            }

            if (projeto.ResponsavelId.HasValue)
            {
                var responsavelAtivo = await _context.Usuarios
                    .Where(u => u.Id == projeto.ResponsavelId.Value)
                    .Select(u => u.Ativo)
                    .FirstOrDefaultAsync();

                if (!responsavelAtivo)
                {
                    result.AddError("Responsável selecionado não está ativo");
                }
            }
        }

        private async Task ValidateForeignKeys(ProjetoViewModel projeto, ValidationResult result)
        {
            var statusExiste = await _context.StatusProjetos.AnyAsync(s => s.Id == projeto.StatusId);
            if (!statusExiste)
            {
                result.AddError("Status selecionado não existe");
            }

            if (projeto.ResponsavelId.HasValue)
            {
                var responsavelExiste = await _context.Usuarios.AnyAsync(u => u.Id == projeto.ResponsavelId.Value);
                if (!responsavelExiste)
                {
                    result.AddError("Responsável selecionado não existe");
                }
            }
        }

        private async Task ValidateUniqueConstraints(ProjetoViewModel projeto, ValidationResult result, int? excludeId = null)
        {
            var query = _context.Projetos.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            var nomeExiste = await query.AnyAsync(p => p.Nome.ToLower() == projeto.Nome.ToLower());
            if (nomeExiste)
            {
                result.AddError("Já existe um projeto com este nome");
            }
        }

        private async Task ValidateStatusTransition(string? statusAtual, string statusNovo, Projeto projeto, ValidationResult result)
        {
            if (string.IsNullOrEmpty(statusAtual))
                return;

            var transitionsInvalidas = new Dictionary<string, HashSet<string>>
            {
                ["Concluído"] = new HashSet<string> { "Planejamento", "Em Andamento", "Pausado" },
                ["Cancelado"] = new HashSet<string> { "Planejamento", "Em Andamento", "Pausado", "Concluído" }
            };

            if (transitionsInvalidas.ContainsKey(statusAtual) &&
                transitionsInvalidas[statusAtual].Contains(statusNovo))
            {
                result.AddError($"Não é possível alterar status de '{statusAtual}' para '{statusNovo}'");
            }

            if (statusNovo == "Concluído")
            {
                var atividadesPendentes = projeto.Atividades?
                    .Where(a => a.Status?.Nome != "Concluída" && a.Status?.Nome != "Cancelada")
                    .Count() ?? 0;

                if (atividadesPendentes > 0)
                {
                    result.AddError($"Não é possível concluir projeto com {atividadesPendentes} atividade(s) pendente(s)");
                }
            }

            await Task.CompletedTask;
        }
    }
}