using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Equipe;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Atividades
{
    /// <summary>
    /// Service para operações de lookup
    /// </summary>
    public class LookupService : ILookupService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<LookupService> _logger;

        public LookupService(ProGestaoContext context, ILogger<LookupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync()
        {
            var projetos = await _context.Projetos
                .Include(p => p.Status)
                .Where(p => p.Status.Nome != "Cancelado")
                .OrderBy(p => p.Nome)
                .AsNoTracking()
                .ToListAsync();

            return projetos.Select(p => new ProjetoViewModel
            {
                Id = p.Id,
                Nome = p.Nome,
                StatusNome = p.Status?.Nome
            }).ToList();
        }

        public async Task<IList<UsuarioViewModel>> GetUsuariosAtivosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Equipe)
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .AsNoTracking()
                .ToListAsync();

            return usuarios.Select(u => new UsuarioViewModel
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Cargo = u.Cargo,
                EquipeNome = u.Equipe?.Nome
            }).ToList();
        }

        public async Task<IList<StatusAtividadeViewModel>> GetStatusAtividadesAsync()
        {
            var status = await _context.StatusAtividades
                .OrderBy(s => s.Ordem)
                .AsNoTracking()
                .ToListAsync();

            return status.Select(s => new StatusAtividadeViewModel
            {
                Id = s.Id,
                Nome = s.Nome,
                Cor = s.Cor,
                Ordem = s.Ordem
            }).ToList();
        }

        public async Task<IList<TipoAtividadeViewModel>> GetTiposAtividadeAsync()
        {
            var tipos = await _context.TiposAtividade
                .OrderBy(t => t.Nome)
                .AsNoTracking()
                .ToListAsync();

            return tipos.Select(t => new TipoAtividadeViewModel
            {
                Id = t.Id,
                Nome = t.Nome,
                Cor = t.Cor,
                Descricao = t.Descricao
            }).ToList();
        }

        public async Task<IList<EquipeViewModel>> GetEquipesAtivasAsync()
        {
            var equipes = await _context.Equipes
                .OrderBy(e => e.Nome)
                .AsNoTracking()
                .ToListAsync();

            return equipes.Select(e => new EquipeViewModel
            {
                Id = e.Id,
                Nome = e.Nome,
                Descricao = e.Descricao
            }).ToList();
        }
    }



}
