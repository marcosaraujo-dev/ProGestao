using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Projetos;
namespace ProGestao.Services
{
    /*
    public interface IProjetoService
    {
        Task<IList<ProjetoViewModel>> GetProjetosAsync();
        Task<IList<ProjetoViewModel>> GetProjetosByFiltroAsync(string? filtroStatus = null, int? responsavelId = null, string? filtroNome = null);
        Task<ProjetoViewModel?> GetProjetoByIdAsync(int id);
        Task<ProjetoViewModel?> GetProjetoComAtividadesAsync(int id);
        Task<bool> CreateProjetoAsync(ProjetoViewModel projeto);
        Task<bool> UpdateProjetoAsync(ProjetoViewModel projeto);
        Task<bool> DeleteProjetoAsync(int id);
        Task<bool> CanDeleteProjetoAsync(int id);
        Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync();
        Task<IList<ProjetoViewModel>> GetProjetosPorResponsavelAsync(int responsavelId);
        Task<decimal> GetProgressoProjetoAsync(int projetoId);
        Task<bool> AlterarStatusProjetoAsync(int projetoId, int novoStatusId);
        Task<bool> AtribuirResponsavelAsync(int projetoId, int? responsavelId);
        Task<ProjetoResumoViewModel> GetResumoProjetoAsync(int projetoId);
        Task<IList<ProjetoViewModel>> GetProjetosVencendoAsync(int diasAntecedencia = 7);
        Task<IList<ProjetoViewModel>> GetProjetosAtrasadosAsync();
    }

    public class ProjetoService : IProjetoService
        {
            private readonly ProGestaoContext _context;
            private readonly ILogger<ProjetoService> _logger;

            public ProjetoService(ProGestaoContext context, ILogger<ProjetoService> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosAsync()
            {
                try
                {
                    var projetos = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .OrderByDescending(p => p.DataCriacao)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos");
                    throw;
                }
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosByFiltroAsync(string? filtroStatus = null, int? responsavelId = null, string? filtroNome = null)
            {
                try
                {
                    var query = _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .AsQueryable();

                    // Aplicar filtros
                    if (!string.IsNullOrEmpty(filtroStatus))
                    {
                        query = query.Where(p => p.Status.Nome.Contains(filtroStatus));
                    }

                    if (responsavelId.HasValue)
                    {
                        query = query.Where(p => p.ResponsavelId == responsavelId.Value);
                    }

                    if (!string.IsNullOrEmpty(filtroNome))
                    {
                        query = query.Where(p => p.Nome.Contains(filtroNome) ||
                                               (p.Descricao != null && p.Descricao.Contains(filtroNome)));
                    }

                    var projetos = await query
                        .OrderByDescending(p => p.DataCriacao)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos com filtros. Status: {Status}, Responsável: {Responsavel}, Nome: {Nome}",
                        filtroStatus, responsavelId, filtroNome);
                    throw;
                }
            }

            public async Task<ProjetoViewModel?> GetProjetoByIdAsync(int id)
            {
                try
                {
                    var projeto = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (projeto == null)
                        return null;

                    return await MapToViewModelAsync(projeto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projeto por ID: {ProjetoId}", id);
                    throw;
                }
            }

            public async Task<ProjetoViewModel?> GetProjetoComAtividadesAsync(int id)
            {
                try
                {
                    var projeto = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .Include(p => p.Atividades)
                            .ThenInclude(a => a.Status)
                        .Include(p => p.Atividades)
                            .ThenInclude(a => a.Usuario)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (projeto == null)
                        return null;

                    return await MapToViewModelAsync(projeto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projeto com atividades. ID: {ProjetoId}", id);
                    throw;
                }
            }

            public async Task<bool> CreateProjetoAsync(ProjetoViewModel projetoVm)
            {
                try
                {
                    var projeto = new Projeto
                    {
                        Nome = projetoVm.Nome,
                        Descricao = projetoVm.Descricao,
                        DataInicio = projetoVm.DataInicio,
                        DataFimPrevista = projetoVm.DataFimPrevista,
                        DataFimReal = projetoVm.DataFimReal,
                        StatusId = projetoVm.StatusId,
                        ResponsavelId = projetoVm.ResponsavelId,
                        LinkProjeto = projetoVm.LinkProjeto,
                        Observacoes = projetoVm.Observacoes,
                        DataCriacao = DateTime.Now
                    };

                    _context.Projetos.Add(projeto);
                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation("Projeto criado com sucesso. ID: {ProjetoId}, Nome: {Nome}", projeto.Id, projeto.Nome);
                    return result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar projeto: {Nome}", projetoVm.Nome);
                    throw;
                }
            }

            public async Task<bool> UpdateProjetoAsync(ProjetoViewModel projetoVm)
            {
                try
                {
                    var projeto = await _context.Projetos.FindAsync(projetoVm.Id);
                    if (projeto == null)
                    {
                        _logger.LogWarning("Tentativa de atualizar projeto inexistente. ID: {ProjetoId}", projetoVm.Id);
                        return false;
                    }

                    // Guardar valores anteriores para log
                    var nomeAnterior = projeto.Nome;
                    var statusAnterior = projeto.StatusId;

                    // Atualizar campos
                    projeto.Nome = projetoVm.Nome;
                    projeto.Descricao = projetoVm.Descricao;
                    projeto.DataInicio = projetoVm.DataInicio;
                    projeto.DataFimPrevista = projetoVm.DataFimPrevista;
                    projeto.DataFimReal = projetoVm.DataFimReal;
                    projeto.StatusId = projetoVm.StatusId;
                    projeto.ResponsavelId = projetoVm.ResponsavelId;
                    projeto.LinkProjeto = projetoVm.LinkProjeto;
                    projeto.Observacoes = projetoVm.Observacoes;

                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation("Projeto atualizado. ID: {ProjetoId}, Nome: {NomeAnterior} -> {NovoNome}, Status: {StatusAnterior} -> {NovoStatus}",
                        projeto.Id, nomeAnterior, projeto.Nome, statusAnterior, projeto.StatusId);

                    return result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar projeto ID: {ProjetoId}", projetoVm.Id);
                    throw;
                }
            }

            public async Task<bool> DeleteProjetoAsync(int id)
            {
                try
                {
                    var projeto = await _context.Projetos
                        .Include(p => p.Atividades)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (projeto == null)
                    {
                        _logger.LogWarning("Tentativa de excluir projeto inexistente. ID: {ProjetoId}", id);
                        return false;
                    }

                    // Verificar se pode excluir
                    if (!await CanDeleteProjetoAsync(id))
                    {
                        _logger.LogWarning("Tentativa de excluir projeto com atividades. ID: {ProjetoId}", id);
                        return false;
                    }

                    var nomeProjeto = projeto.Nome;
                    _context.Projetos.Remove(projeto);
                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation("Projeto excluído com sucesso. ID: {ProjetoId}, Nome: {Nome}", id, nomeProjeto);
                    return result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao excluir projeto ID: {ProjetoId}", id);
                    throw;
                }
            }

            public async Task<bool> CanDeleteProjetoAsync(int id)
            {
                try
                {
                    var temAtividades = await _context.Atividades
                        .AnyAsync(a => a.ProjetoId == id);

                    return !temAtividades;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao verificar se projeto pode ser excluído. ID: {ProjetoId}", id);
                    throw;
                }
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync()
            {
                try
                {
                    var projetos = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .Where(p => p.Status.Nome != "Concluído" && p.Status.Nome != "Cancelado")
                        .OrderBy(p => p.Nome)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos ativos");
                    throw;
                }
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosPorResponsavelAsync(int responsavelId)
            {
                try
                {
                    var projetos = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .Where(p => p.ResponsavelId == responsavelId)
                        .OrderByDescending(p => p.DataCriacao)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos por responsável: {ResponsavelId}", responsavelId);
                    throw;
                }
            }

            public async Task<decimal> GetProgressoProjetoAsync(int projetoId)
            {
                try
                {
                    var totalAtividades = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId)
                        .CountAsync();

                    if (totalAtividades == 0)
                        return 0;

                    var atividadesConcluidas = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId && a.Status.Nome == "Concluída")
                        .CountAsync();

                    return Math.Round((decimal)atividadesConcluidas / totalAtividades * 100, 2);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao calcular progresso do projeto: {ProjetoId}", projetoId);
                    throw;
                }
            }

            public async Task<bool> AlterarStatusProjetoAsync(int projetoId, int novoStatusId)
            {
                try
                {
                    var projeto = await _context.Projetos.FindAsync(projetoId);
                    if (projeto == null)
                        return false;

                    var statusAnterior = projeto.StatusId;
                    projeto.StatusId = novoStatusId;

                    // Se o status for "Concluído", definir data de fim real
                    var novoStatus = await _context.StatusProjetos.FindAsync(novoStatusId);
                    if (novoStatus?.Nome == "Concluído" && projeto.DataFimReal == null)
                    {
                        projeto.DataFimReal = DateTime.Now;
                    }

                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation("Status do projeto alterado. ID: {ProjetoId}, Status: {StatusAnterior} -> {NovoStatus}",
                        projetoId, statusAnterior, novoStatusId);

                    return result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao alterar status do projeto: {ProjetoId}", projetoId);
                    throw;
                }
            }

            public async Task<bool> AtribuirResponsavelAsync(int projetoId, int? responsavelId)
            {
                try
                {
                    var projeto = await _context.Projetos.FindAsync(projetoId);
                    if (projeto == null)
                        return false;

                    var responsavelAnterior = projeto.ResponsavelId;
                    projeto.ResponsavelId = responsavelId;

                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation("Responsável do projeto alterado. ID: {ProjetoId}, Responsável: {ResponsavelAnterior} -> {NovoResponsavel}",
                        projetoId, responsavelAnterior, responsavelId);

                    return result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atribuir responsável ao projeto: {ProjetoId}", projetoId);
                    throw;
                }
            }

            public async Task<ProjetoResumoViewModel> GetResumoProjetoAsync(int projetoId)
            {
                try
                {
                    var projeto = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .FirstOrDefaultAsync(p => p.Id == projetoId);

                    if (projeto == null)
                        throw new ArgumentException($"Projeto com ID {projetoId} não encontrado");

                    var totalAtividades = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId)
                        .CountAsync();

                    var atividadesPorStatus = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId)
                        .Include(a => a.Status)
                        .GroupBy(a => a.Status.Nome)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var atrasadas = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId &&
                                   a.DataFimPrevista < DateTime.Today &&
                                   a.DataFimReal == null &&
                                   a.Status.Nome != "Concluída")
                        .CountAsync();

                    var horasEstimadas = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId)
                        .SumAsync(a => a.HorasEstimadas ?? 0);

                    var horasReais = await _context.Atividades
                        .Where(a => a.ProjetoId == projetoId)
                        .SumAsync(a => a.HorasReais ?? 0);

                    return new ProjetoResumoViewModel
                    {
                        ProjetoId = projeto.Id,
                        NomeProjeto = projeto.Nome,
                        StatusProjeto = projeto.Status?.Nome,
                        ResponsavelNome = projeto.Responsavel?.Nome,
                        DataInicio = projeto.DataInicio,
                        DataFimPrevista = projeto.DataFimPrevista,
                        DataFimReal = projeto.DataFimReal,
                        TotalAtividades = totalAtividades,
                        AtividadesPendentes = atividadesPorStatus.FirstOrDefault(x => x.Status == "Pendente")?.Count ?? 0,
                        AtividadesAndamento = atividadesPorStatus.FirstOrDefault(x => x.Status == "Em Andamento")?.Count ?? 0,
                        AtividadesConcluidas = atividadesPorStatus.FirstOrDefault(x => x.Status == "Concluída")?.Count ?? 0,
                        AtividadesAtrasadas = atrasadas,
                        HorasEstimadas = horasEstimadas,
                        HorasReais = horasReais,
                        ProgressoPercentual = totalAtividades > 0 ?
                            Math.Round((decimal)(atividadesPorStatus.FirstOrDefault(x => x.Status == "Concluída")?.Count ?? 0) / totalAtividades * 100, 2) : 0
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter resumo do projeto: {ProjetoId}", projetoId);
                    throw;
                }
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosVencendoAsync(int diasAntecedencia = 7)
            {
                try
                {
                    var dataLimite = DateTime.Today.AddDays(diasAntecedencia);

                    var projetos = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .Where(p => p.DataFimPrevista <= dataLimite &&
                                   p.DataFimPrevista >= DateTime.Today &&
                                   p.DataFimReal == null &&
                                   p.Status.Nome != "Concluído" &&
                                   p.Status.Nome != "Cancelado")
                        .OrderBy(p => p.DataFimPrevista)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos vencendo em {Dias} dias", diasAntecedencia);
                    throw;
                }
            }

            public async Task<IList<ProjetoViewModel>> GetProjetosAtrasadosAsync()
            {
                try
                {
                    var hoje = DateTime.Today;

                    var projetos = await _context.Projetos
                        .Include(p => p.Status)
                        .Include(p => p.Responsavel)
                        .Where(p => p.DataFimPrevista < hoje &&
                                   p.DataFimReal == null &&
                                   p.Status.Nome != "Concluído" &&
                                   p.Status.Nome != "Cancelado")
                        .OrderBy(p => p.DataFimPrevista)
                        .ToListAsync();

                    var projetosViewModel = new List<ProjetoViewModel>();

                    foreach (var projeto in projetos)
                    {
                        var projetoViewModel = await MapToViewModelAsync(projeto);
                        projetosViewModel.Add(projetoViewModel);
                    }

                    return projetosViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar projetos atrasados");
                    throw;
                }
            }

            private async Task<ProjetoViewModel> MapToViewModelAsync(Projeto projeto)
            {
                var totalAtividades = await _context.Atividades
                    .Where(a => a.ProjetoId == projeto.Id)
                    .CountAsync();

                var atividadesConcluidas = await _context.Atividades
                    .Where(a => a.ProjetoId == projeto.Id && a.Status.Nome == "Concluída")
                    .CountAsync();

                return new ProjetoViewModel
                {
                    Id = projeto.Id,
                    Nome = projeto.Nome,
                    Descricao = projeto.Descricao,
                    DataInicio = projeto.DataInicio,
                    DataFimPrevista = projeto.DataFimPrevista,
                    DataFimReal = projeto.DataFimReal,
                    StatusId = projeto.StatusId,
                    StatusNome = projeto.Status.Nome,
                    StatusCor = projeto.Status.Cor,
                    ResponsavelId = projeto.ResponsavelId,
                    ResponsavelNome = projeto.Responsavel?.Nome,
                    LinkProjeto = projeto.LinkProjeto,
                    Observacoes = projeto.Observacoes,
                    QtdAtividades = totalAtividades,
                    QtdAtividadesConcluidas = atividadesConcluidas,
                    DataCriacao = projeto.DataCriacao
                };
            }
        }
        */
    }



