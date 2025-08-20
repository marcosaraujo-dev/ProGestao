using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Atividades
{

    /// <summary>
    /// Implementação do mapper específico para Atividades
    /// Segue Single Responsibility Principle
    /// </summary>
    public class AtividadeMapper : IAtividadeMapper
    {
        private readonly ILogger<AtividadeMapper> _logger;

        public AtividadeMapper(ILogger<AtividadeMapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AtividadeViewModel MapToViewModel(Atividade atividade)
        {
            if (atividade == null)
                throw new ArgumentNullException(nameof(atividade));

            return new AtividadeViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                Descricao = atividade.Descricao,
                ProjetoId = atividade.ProjetoId,
                ProjetoNome = atividade.Projeto?.Nome,
                TipoAtividadeId = atividade.TipoAtividadeId,
                TipoAtividadeNome = atividade.TipoAtividade?.Nome,
                StatusId = atividade.StatusId,
                StatusNome = atividade.Status?.Nome,
                StatusCor = atividade.Status?.Cor,
                UsuarioId = atividade.UsuarioId,
                UsuarioNome = atividade.Usuario?.Nome,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimPrevista,
                DataFimReal = atividade.DataFimReal,
                HorasEstimadas = atividade.HorasEstimadas,
                HorasReais = atividade.HorasReais,
                Prioridade = atividade.Prioridade,
                Observacoes = atividade.Observacoes,
                DataCriacao = atividade.DataCriacao,
                DataAtualizacao = atividade.DataAtualizacao
            };
        }

        public AtividadeViewModel MapToViewModelDetalhado(Atividade atividade)
        {
            if (atividade == null)
                throw new ArgumentNullException(nameof(atividade));

            var viewModel = MapToViewModel(atividade);

            // Informações adicionais detalhadas
            if (atividade.Usuario?.Equipe != null)
            {
                viewModel.UsuarioEquipe = atividade.Usuario.Equipe.Nome;
                viewModel.UsuarioEquipeId = atividade.Usuario.Equipe.Id;
            }

            if (atividade.Projeto?.Status != null)
            {
                viewModel.ProjetoStatus = atividade.Projeto.Status.Nome;
                viewModel.ProjetoStatusCor = atividade.Projeto.Status.Cor;
            }

            if (atividade.Projeto?.Responsavel != null)
            {
                viewModel.ProjetoResponsavel = atividade.Projeto.Responsavel.Nome;
            }

            return viewModel;
        }

        public AtividadeViewModel MapToViewModelSimples(Atividade atividade)
        {
            if (atividade == null)
                throw new ArgumentNullException(nameof(atividade));

            // Versão simplificada para listagens grandes
            return new AtividadeViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                StatusId = atividade.StatusId,
                StatusNome = atividade.Status?.Nome,
                StatusCor = atividade.Status?.Cor,
                UsuarioId = atividade.UsuarioId,
                UsuarioNome = atividade.Usuario?.Nome,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimPrevista,
                Prioridade = atividade.Prioridade,
                ProjetoNome = atividade.Projeto?.Nome
            };
        }

        public AtividadeGridViewModel MapToGridViewModel(Atividade atividade)
        {
            if (atividade == null)
                throw new ArgumentNullException(nameof(atividade));

            return new AtividadeGridViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                StatusNome = atividade.Status?.Nome,
                StatusCor = atividade.Status?.Cor,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimReal ?? atividade.DataFimPrevista,
                Prioridade = atividade.Prioridade,
                UsuarioId = atividade.UsuarioId,
                ProjetoNome = atividade.Projeto?.Nome
            };
        }

        public IList<AtividadeViewModel> MapToViewModelList(IList<Atividade> atividades)
        {
            if (atividades == null)
                return new List<AtividadeViewModel>();

            try
            {
                return atividades.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mapear lista de atividades");
                throw;
            }
        }

        public Atividade MapToEntity(AtividadeViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return new Atividade
            {
                Nome = viewModel.Nome,
                Descricao = viewModel.Descricao,
                ProjetoId = viewModel.ProjetoId,
                TipoAtividadeId = viewModel.TipoAtividadeId,
                StatusId = viewModel.StatusId,
                UsuarioId = viewModel.UsuarioId,
                DataInicio = viewModel.DataInicio,
                DataFimPrevista = viewModel.DataFimPrevista,
                DataFimReal = viewModel.DataFimReal,
                HorasEstimadas = viewModel.HorasEstimadas,
                HorasReais = viewModel.HorasReais,
                Prioridade = viewModel.Prioridade,
                Observacoes = viewModel.Observacoes
            };
        }

        public void MapToEntityUpdate(AtividadeViewModel viewModel, Atividade entity)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Nome = viewModel.Nome;
            entity.Descricao = viewModel.Descricao;
            entity.ProjetoId = viewModel.ProjetoId;
            entity.TipoAtividadeId = viewModel.TipoAtividadeId;
            entity.StatusId = viewModel.StatusId;
            entity.UsuarioId = viewModel.UsuarioId;
            entity.DataInicio = viewModel.DataInicio;
            entity.DataFimPrevista = viewModel.DataFimPrevista;
            entity.DataFimReal = viewModel.DataFimReal;
            entity.HorasEstimadas = viewModel.HorasEstimadas;
            entity.HorasReais = viewModel.HorasReais;
            entity.Prioridade = viewModel.Prioridade;
            entity.Observacoes = viewModel.Observacoes;
        }
    }
}
