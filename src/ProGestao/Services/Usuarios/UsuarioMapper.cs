using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Usuarios
{
    /// <summary>
    /// Implementação do mapper específico para Usuários
    /// </summary>
    public class UsuarioMapper : IUsuarioMapper
    {
        private readonly ILogger<UsuarioMapper> _logger;

        public UsuarioMapper(ILogger<UsuarioMapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public UsuarioViewModel MapToViewModel(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            return new UsuarioViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Cargo = usuario.Cargo,
                EquipeId = usuario.EquipeId,
                EquipeNome = usuario.Equipe?.Nome,
                Ativo = usuario.Ativo,
                DataAdmissao = usuario.DataAdmissao
            };
        }

        public UsuarioViewModel MapToViewModelComEquipe(Usuario usuario)
        {
            var viewModel = MapToViewModel(usuario);

            if (usuario.Equipe != null)
            {
                viewModel.EquipeNome = usuario.Equipe.Descricao;
            }

            return viewModel;
        }

        public UsuarioLookupViewModel MapToLookupViewModel(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            return new UsuarioLookupViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Cargo = usuario.Cargo,
                EquipeNome = usuario.Equipe?.Nome
            };
        }

        public UsuarioPerformanceViewModel MapToPerformanceViewModel(Usuario usuario, IList<Atividade> atividades)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            var atividadesUsuario = atividades?.Where(a => a.UsuarioId == usuario.Id).ToList() ?? new List<Atividade>();

            return new UsuarioPerformanceViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                EquipeNome = usuario.Equipe?.Nome,
                TotalAtividades = atividadesUsuario.Count,
                AtividadesConcluidas = atividadesUsuario.Count(a => a.Status?.Nome == "Concluída"),
                AtividadesEmAndamento = atividadesUsuario.Count(a => a.Status?.Nome == "Em Andamento"),
                AtividadesAtrasadas = atividadesUsuario.Count(a =>
                    a.DataFimPrevista.HasValue &&
                    a.DataFimPrevista < DateTime.Today &&
                    a.Status?.Nome != "Concluída"),
                TaxaConclusao = atividadesUsuario.Count > 0
                    ? (decimal)atividadesUsuario.Count(a => a.Status?.Nome == "Concluída") / atividadesUsuario.Count * 100
                    : 0
            };
        }

        public IList<UsuarioViewModel> MapToViewModelList(IList<Usuario> usuarios)
        {
            if (usuarios == null)
                return new List<UsuarioViewModel>();

            return usuarios.Select(MapToViewModel).ToList();
        }

        public Usuario MapToEntity(UsuarioViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return new Usuario
            {
                Nome = viewModel.Nome,
                Email = viewModel.Email,
                Cargo = viewModel.Cargo,
                EquipeId = viewModel.EquipeId,
                Ativo = viewModel.Ativo,
                DataAdmissao = viewModel.DataAdmissao
            };
        }

        public void MapToEntityUpdate(UsuarioViewModel viewModel, Usuario entity)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Nome = viewModel.Nome;
            entity.Email = viewModel.Email;
            entity.Cargo = viewModel.Cargo;
            entity.EquipeId = viewModel.EquipeId;
            entity.Ativo = viewModel.Ativo;
            entity.DataAdmissao = viewModel.DataAdmissao;
        }
    }
}
