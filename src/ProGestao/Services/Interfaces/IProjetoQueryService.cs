using ProGestao.Common;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de leitura de projetos
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IProjetoQueryService
    {
        Task<IList<ProjetoViewModel>> GetProjetosAsync();
        Task<IList<ProjetoViewModel>> GetProjetosByFiltroAsync(string? filtroStatus = null, int? responsavelId = null, string? filtroNome = null);
        Task<ProjetoViewModel?> GetProjetoByIdAsync(int id);
        Task<ProjetoViewModel?> GetProjetoComDetalhesAsync(int id);
        Task<ProjetoViewModel?> GetProjetoComAtividadesAsync(int id);
        Task<bool> ExisteProjetoAsync(int id);
        Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync();
        Task<IList<ProjetoViewModel>> GetProjetosPorResponsavelAsync(int responsavelId);
        Task<IList<ProjetoViewModel>> GetProjetosVencendoAsync(int diasAntecedencia = 7);
        Task<IList<ProjetoViewModel>> GetProjetosAtrasadosAsync();
        Task<decimal> GetProgressoProjetoAsync(int projetoId);
        Task<int> GetQuantidadeAtividadesAsync(int projetoId);
    }

    /// <summary>
    /// Interface para operações de escrita de projetos
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IProjetoCommandService
    {
        Task<Result<int>> CreateProjetoAsync(ProjetoViewModel projeto);
        Task<Result<bool>> UpdateProjetoAsync(ProjetoViewModel projeto);
        Task<Result<bool>> DeleteProjetoAsync(int id);
        Task<Result<bool>> AlterarStatusProjetoAsync(int projetoId, int novoStatusId);
        Task<Result<bool>> AtribuirResponsavelAsync(int projetoId, int? responsavelId);
        Task<Result<bool>> FinalizarProjetoAsync(int projetoId, DateTime? dataFim = null);
        Task<Result<bool>> CancelarProjetoAsync(int projetoId, string? motivoCancelamento = null);
    }
}
