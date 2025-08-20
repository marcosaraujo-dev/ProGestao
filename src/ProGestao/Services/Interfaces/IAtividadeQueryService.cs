using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de leitura de atividades
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAtividadeQueryService
    {
        Task<IList<AtividadeViewModel>> GetAtividadesByFiltroAsync(int? projetoId = null, int? usuarioId = null, int? statusId = null);
        Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id);
        Task<AtividadeViewModel?> GetAtividadeComDetalhesAsync(int id);
        Task<bool> ExisteAtividadeAsync(int id);
        Task<IList<AtividadeViewModel>> GetAtividadesAtivasAsync();
        Task<IList<AtividadeViewModel>> GetAtividadesPorUsuarioAsync(int usuarioId);
        Task<IList<AtividadeViewModel>> GetAtividadesPorProjetoAsync(int projetoId);
    }
}