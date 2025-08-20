using ProGestao.ViewModels.Atividade;
using System.ComponentModel.DataAnnotations;

public interface IAtividadeService
{
    public interface IAtividadeService
    {
        Task<IList<AtividadeViewModel>> GetAtividadesByFiltroAsync(int? projetoId = null, int? usuarioId = null, int? statusId = null);
        Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id);
        Task<bool> CreateAtividadeAsync(AtividadeViewModel atividade);
        Task<bool> UpdateAtividadeAsync(AtividadeViewModel atividade);
        Task<bool> DeleteAtividadeAsync(int id);
    }
}