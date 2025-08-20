
namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface base genérica para mappers (opcional)
    /// Permite reutilização de padrões comuns
    /// </summary>
    public interface IBaseMapper<TEntity, TViewModel>
        where TEntity : class
        where TViewModel : class
    {
        TViewModel MapToViewModel(TEntity entity);
        IList<TViewModel> MapToViewModelList(IList<TEntity> entities);
        TEntity MapToEntity(TViewModel viewModel);
        void MapToEntityUpdate(TViewModel viewModel, TEntity entity);
    }
}
