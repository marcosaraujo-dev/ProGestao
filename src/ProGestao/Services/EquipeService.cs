using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Services
{
    public interface IEquipeService
    {
        Task<IList<Equipe>> GetEquipesAsync();
        Task<IList<Equipe>> GetEquipesAtivasAsync();
        Task<Equipe?> GetEquipeByIdAsync(int id);
        Task<bool> CreateEquipeAsync(Equipe equipe);
        Task<bool> UpdateEquipeAsync(Equipe equipe);
        Task<bool> DeleteEquipeAsync(int id);
        Task<bool> EquipeExisteAsync(int id);
        Task<bool> PodeExcluirEquipeAsync(int id);
    }

    public class EquipeService : IEquipeService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<EquipeService> _logger;

        public EquipeService(ProGestaoContext context, ILogger<EquipeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IList<Equipe>> GetEquipesAsync()
        {
            try
            {
                return await _context.Equipes
                    .OrderBy(e => e.Nome)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipes");
                return new List<Equipe>();
            }
        }

        public async Task<IList<Equipe>> GetEquipesAtivasAsync()
        {
            try
            {
                return await _context.Equipes
                    .Where(e => e.Ativo)
                    .OrderBy(e => e.Nome)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipes ativas");
                return new List<Equipe>();
            }
        }

        public async Task<Equipe?> GetEquipeByIdAsync(int id)
        {
            try
            {
                return await _context.Equipes.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipe por ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateEquipeAsync(Equipe equipe)
        {
            try
            {
                if (string.IsNullOrEmpty(equipe.Nome))
                {
                    _logger.LogWarning("Tentativa de criar equipe sem nome");
                    return false;
                }

                // Verificar se já existe equipe com mesmo nome
                var equipeExistente = await _context.Equipes
                    .FirstOrDefaultAsync(e => e.Nome.ToLower() == equipe.Nome.ToLower());

                if (equipeExistente != null)
                {
                    _logger.LogWarning("Já existe uma equipe com o nome: {Nome}", equipe.Nome);
                    return false;
                }

                equipe.DataCriacao = DateTime.Now;
                _context.Equipes.Add(equipe);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Equipe criada com sucesso: {Nome} (ID: {Id})", equipe.Nome, equipe.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar equipe: {Nome}", equipe.Nome);
                return false;
            }
        }

        public async Task<bool> UpdateEquipeAsync(Equipe equipe)
        {
            try
            {
                var equipeExistente = await _context.Equipes.FindAsync(equipe.Id);
                if (equipeExistente == null)
                {
                    _logger.LogWarning("Equipe com ID {Id} não encontrada para atualização", equipe.Id);
                    return false;
                }

                // Verificar se já existe outra equipe com mesmo nome
                var outraEquipe = await _context.Equipes
                    .FirstOrDefaultAsync(e => e.Nome.ToLower() == equipe.Nome.ToLower() && e.Id != equipe.Id);

                if (outraEquipe != null)
                {
                    _logger.LogWarning("Já existe outra equipe com o nome: {Nome}", equipe.Nome);
                    return false;
                }

                equipeExistente.Nome = equipe.Nome;
                equipeExistente.Descricao = equipe.Descricao;
                equipeExistente.Ativo = equipe.Ativo;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Equipe atualizada com sucesso: {Nome} (ID: {Id})", equipe.Nome, equipe.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar equipe ID: {Id}", equipe.Id);
                return false;
            }
        }

        public async Task<bool> DeleteEquipeAsync(int id)
        {
            try
            {
                var equipe = await _context.Equipes.FindAsync(id);
                if (equipe == null)
                {
                    _logger.LogWarning("Equipe com ID {Id} não encontrada para exclusão", id);
                    return false;
                }

                if (!await PodeExcluirEquipeAsync(id))
                {
                    _logger.LogWarning("Não é possível excluir a equipe {Id} pois possui usuários vinculados", id);
                    return false;
                }

                _context.Equipes.Remove(equipe);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Equipe excluída com sucesso: {Nome} (ID: {Id})", equipe.Nome, equipe.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir equipe ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> EquipeExisteAsync(int id)
        {
            try
            {
                return await _context.Equipes.AnyAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se equipe existe: {Id}", id);
                return false;
            }
        }

        public async Task<bool> PodeExcluirEquipeAsync(int id)
        {
            try
            {
                var temUsuarios = await _context.Usuarios.AnyAsync(u => u.EquipeId == id);
                return !temUsuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se equipe pode ser excluída: {Id}", id);
                return false;
            }
        }
    }
}
