
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Implementation
{

    /// <summary>
    /// Service para operações de lookup de projetos
    /// </summary>
    public class ProjetoLookupService : IProjetoLookupService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<ProjetoLookupService> _logger;

        public ProjetoLookupService(ProGestaoContext context, ILogger<ProjetoLookupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<ProjetoLookupViewModel>> GetProjetosForDropdownAsync()
        {
            try
            {
                var projetos = await _context.Projetos
                    .Include(p => p.Status)
                    .Where(p => p.Status.Nome != "Cancelado")
                    .OrderBy(p => p.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                return projetos.Select(p => new ProjetoLookupViewModel
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    StatusNome = p.Status?.Nome
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos para dropdown");
                throw;
            }
        }

        public async Task<IList<StatusProjetoViewModel>> GetStatusProjetosAsync()
        {
            try
            {
                var status = await _context.StatusProjetos
                    .OrderBy(s => s.Ordem)
                    .AsNoTracking()
                    .ToListAsync();

                return status.Select(s => new StatusProjetoViewModel
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Cor = s.Cor,
                    Ordem = s.Ordem
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de projetos");
                throw;
            }
        }

        public async Task<IList<UsuarioLookupViewModel>> GetResponsaveisAsync()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                return usuarios.Select(u => new UsuarioLookupViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Cargo = u.Cargo,
                    EquipeNome = u.Equipe?.Nome
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar responsáveis");
                throw;
            }
        }

        public async Task<ProjetoResumoViewModel?> GetResumoProjetoAsync(int projetoId)
        {
            try
            {
                var projeto = await _context.Projetos
                    .Include(p => p.Status)
                    .Include(p => p.Responsavel)
                    .Include(p => p.Atividades)
                        .ThenInclude(a => a.Status)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projetoId);

                if (projeto == null)
                    return null;

                var atividades = projeto.Atividades ?? new List<Atividade>();
                var totalAtividades = atividades.Count;
                var atividadesConcluidas = atividades.Count(a => a.Status?.Nome == "Concluída");
                var atividadesEmAndamento = atividades.Count(a => a.Status?.Nome == "Em Andamento");
                var atividadesPendentes = atividades.Count(a => a.Status?.Nome == "Pendente");
                var atividadesAtrasadas = atividades.Count(a =>
                    a.DataFimPrevista.HasValue &&
                    a.DataFimPrevista < DateTime.Today &&
                    a.Status?.Nome != "Concluída");

                var hoje = DateTime.Today;
                var diasRestantes = projeto.DataFimPrevista.HasValue
                    ? (int)(projeto.DataFimPrevista.Value.Date - hoje).TotalDays
                    : 0;

                var totalHorasEstimadas = atividades.Sum(a => a.HorasEstimadas ?? 0);
                var totalHorasReais = atividades.Sum(a => a.HorasReais ?? 0);

                return new ProjetoResumoViewModel
                {
                    Id = projeto.Id,
                    Nome = projeto.Nome,
                    StatusNome = projeto.Status?.Nome,
                    StatusCor = projeto.Status?.Cor,
                    ResponsavelNome = projeto.Responsavel?.Nome,

                    TotalAtividades = totalAtividades,
                    AtividadesConcluidas = atividadesConcluidas,
                    AtividadesEmAndamento = atividadesEmAndamento,
                    AtividadesPendentes = atividadesPendentes,
                    AtividadesAtrasadas = atividadesAtrasadas,

                    PercentualConclusao = totalAtividades > 0 ? Math.Round((decimal)atividadesConcluidas / totalAtividades * 100, 2) : 0,
                    PercentualAndamento = totalAtividades > 0 ? Math.Round((decimal)atividadesEmAndamento / totalAtividades * 100, 2) : 0,

                    DataInicio = projeto.DataInicio,
                    DataFimPrevista = projeto.DataFimPrevista,
                    DataFimReal = projeto.DataFimReal,

                    EstaAtrasado = projeto.DataFimPrevista.HasValue && projeto.DataFimPrevista < hoje && projeto.Status?.Nome != "Concluído",
                    EstaConcluido = projeto.Status?.Nome == "Concluído",
                    EstaEmprejeto = projeto.DataFimPrevista.HasValue && diasRestantes >= 0 && projeto.Status?.Nome != "Concluído",
                    DiasRestantes = diasRestantes,

                    MediaHorasPorAtividade = totalAtividades > 0 && totalHorasReais > 0 ? Math.Round(totalHorasReais / totalAtividades, 2) : 0,
                    TotalHorasEstimadas = totalHorasEstimadas,
                    TotalHorasReais = totalHorasReais
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumo do projeto: {ProjetoId}", projetoId);
                throw;
            }
        }
    }
}