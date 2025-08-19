using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels;

namespace ProGestao.Services
{
    public interface IUsuarioService
    {
        Task<bool> EmailExisteAsync(string email, int? usuarioId = null);
        Task<UsuarioViewModel?> GetUsuarioByIdAsync(int id);
        Task<bool> CreateUsuarioAsync(UsuarioViewModel usuarioVm);
        Task<bool> UpdateUsuarioAsync(UsuarioViewModel usuarioVm);
        Task<bool> DesativarUsuarioAsync(int id);
        Task<bool> AtivarUsuarioAsync(int id);
        Task<IList<UsuarioViewModel>> GetUsuariosAtivosAsync();
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(ProGestaoContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EmailExisteAsync(string email, int? usuarioId = null)
        {
            var query = _context.Usuarios.Where(u => u.Email.ToLower() == email.ToLower());

            if (usuarioId.HasValue)
            {
                query = query.Where(u => u.Id != usuarioId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<UsuarioViewModel?> GetUsuarioByIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Equipe)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return null;

            return MapToViewModel(usuario);
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioViewModel usuarioVm)
        {
            try
            {
                var usuario = new Usuario
                {
                    Nome = usuarioVm.Nome.Trim(),
                    Email = usuarioVm.Email.Trim().ToLower(),
                    Cargo = usuarioVm.Cargo.Trim(),
                    EquipeId = usuarioVm.EquipeId,
                    DataAdmissao = usuarioVm.DataAdmissao,
                    Ativo = usuarioVm.Ativo,
                    DataCriacao = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário criado: {Nome} ({Email})", usuario.Nome, usuario.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário: {Nome}", usuarioVm.Nome);
                return false;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(UsuarioViewModel usuarioVm)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioVm.Id);
                if (usuario == null) return false;

                usuario.Nome = usuarioVm.Nome.Trim();
                usuario.Email = usuarioVm.Email.Trim().ToLower();
                usuario.Cargo = usuarioVm.Cargo.Trim();
                usuario.EquipeId = usuarioVm.EquipeId;
                usuario.DataAdmissao = usuarioVm.DataAdmissao;
                usuario.Ativo = usuarioVm.Ativo;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário atualizado: {Nome} ({Email})", usuario.Nome, usuario.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário: {Id}", usuarioVm.Id);
                return false;
            }
        }

        public async Task<bool> DesativarUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return false;

                usuario.Ativo = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário desativado: {Nome} ({Email})", usuario.Nome, usuario.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar usuário: {Id}", id);
                return false;
            }
        }

        public async Task<bool> AtivarUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return false;

                usuario.Ativo = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário ativado: {Nome} ({Email})", usuario.Nome, usuario.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar usuário: {Id}", id);
                return false;
            }
        }

        public async Task<IList<UsuarioViewModel>> GetUsuariosAtivosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Equipe)
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            return usuarios.Select(MapToViewModel).ToList();
        }

        private static UsuarioViewModel MapToViewModel(Usuario usuario)
        {
            return new UsuarioViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Cargo = usuario.Cargo,
                EquipeId = usuario.EquipeId,
                EquipeNome = usuario.Equipe?.Nome,
                DataAdmissao = usuario.DataAdmissao,
                Ativo = usuario.Ativo,
                DataCriacao = usuario.DataCriacao
            };
        }
    }
}