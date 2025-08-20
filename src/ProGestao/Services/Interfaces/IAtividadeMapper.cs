using ProGestao.Models;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface específica para mapeamento de Atividades
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAtividadeMapper : IBaseMapper<Atividade, AtividadeViewModel>
    {
        /// <summary>
        /// Mapeamento detalhado incluindo informações de relacionamentos
        /// </summary>
        AtividadeViewModel MapToViewModelDetalhado(Atividade atividade);

        /// <summary>
        /// Mapeamento simplificado para listagens
        /// </summary>
        AtividadeViewModel MapToViewModelSimples(Atividade atividade);

        /// <summary>
        /// Mapeamento para grid semanal com informações específicas
        /// </summary>
        AtividadeGridViewModel MapToGridViewModel(Atividade atividade);
    }

}
