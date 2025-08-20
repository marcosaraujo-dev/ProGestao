using ProGestao.Services.Interfaces;
using ProGestao.Models;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Projetos
{
    /// <summary>
    /// Implementação do mapper específico para Projetos
    /// </summary>
    public class ProjetoMapper : IProjetoMapper
    {
        private readonly ILogger<ProjetoMapper> _logger;

        public ProjetoMapper(ILogger<ProjetoMapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ProjetoViewModel MapToViewModel(Projeto projeto)
        {
            if (projeto == null)
                throw new ArgumentNullException(nameof(projeto));

            return new ProjetoViewModel
            {
                Id = projeto.Id,
                Nome = projeto.Nome,
                Descricao = projeto.Descricao,
                DataInicio = projeto.DataInicio,
                DataFimPrevista = projeto.DataFimPrevista,
                DataFimReal = projeto.DataFimReal,
                StatusId = projeto.StatusId,
                StatusNome = projeto.Status?.Nome,
                StatusCor = projeto.Status?.Cor,
                ResponsavelId = projeto.ResponsavelId,
                ResponsavelNome = projeto.Responsavel?.Nome,
                LinkProjeto = projeto.LinkProjeto,
                Observacoes = projeto.Observacoes
            };
        }

        public ProjetoViewModel MapToViewModelComAtividades(Projeto projeto)
        {
            var viewModel = MapToViewModel(projeto);

            if (projeto.Atividades?.Any() == true)
            {
                viewModel.QtdAtividades = projeto.Atividades.Count;
                viewModel.QtdAtividadesConcluidas = projeto.Atividades.Count(a => a.Status?.Nome == "Concluída");
                viewModel.PercentualConclusao = viewModel.QtdAtividades > 0
                    ? (decimal)viewModel.QtdAtividadesConcluidas / viewModel.QtdAtividades * 100
                    : 0;
            }

            return viewModel;
        }

        public ProjetoLookupViewModel MapToLookupViewModel(Projeto projeto)
        {
            if (projeto == null)
                throw new ArgumentNullException(nameof(projeto));

            return new ProjetoLookupViewModel
            {
                Id = projeto.Id,
                Nome = projeto.Nome,
                StatusNome = projeto.Status?.Nome
            };
        }

        public ProjetoDashboardViewModel MapToDashboardViewModel(Projeto projeto)
        {
            var viewModel = MapToViewModelComAtividades(projeto);

            return new ProjetoDashboardViewModel
            {
                Id = viewModel.Id,
                Nome = viewModel.Nome,
                StatusNome = viewModel.StatusNome,
                StatusCor = viewModel.StatusCor,
                PercentualConclusao = viewModel.PercentualConclusao,
                QtdAtividades = viewModel.QtdAtividades,
                DataFimPrevista = viewModel.DataFimPrevista,
                ResponsavelNome = viewModel.ResponsavelNome
            };
        }

        public IList<ProjetoViewModel> MapToViewModelList(IList<Projeto> projetos)
        {
            if (projetos == null)
                return new List<ProjetoViewModel>();

            return projetos.Select(MapToViewModel).ToList();
        }

        public Projeto MapToEntity(ProjetoViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return new Projeto
            {
                Nome = viewModel.Nome,
                Descricao = viewModel.Descricao,
                DataInicio = viewModel.DataInicio,
                DataFimPrevista = viewModel.DataFimPrevista,
                DataFimReal = viewModel.DataFimReal,
                StatusId = viewModel.StatusId,
                ResponsavelId = viewModel.ResponsavelId,
                LinkProjeto = viewModel.LinkProjeto,
                Observacoes = viewModel.Observacoes
            };
        }

        public void MapToEntityUpdate(ProjetoViewModel viewModel, Projeto entity)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Nome = viewModel.Nome;
            entity.Descricao = viewModel.Descricao;
            entity.DataInicio = viewModel.DataInicio;
            entity.DataFimPrevista = viewModel.DataFimPrevista;
            entity.DataFimReal = viewModel.DataFimReal;
            entity.StatusId = viewModel.StatusId;
            entity.ResponsavelId = viewModel.ResponsavelId;
            entity.LinkProjeto = viewModel.LinkProjeto;
            entity.Observacoes = viewModel.Observacoes;
        }
    }
}
