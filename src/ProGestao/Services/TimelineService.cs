using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services
{
    public interface ITimelineService
    {
        Task<IList<TimelineAtividadeViewModel>> GetTimelineAtividadesAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null);
    }

    public class TimelineService : ITimelineService
    {
        private readonly ProGestaoContext _context;

        public TimelineService(ProGestaoContext context)
        {
            _context = context;
        }

        public async Task<IList<TimelineAtividadeViewModel>> GetTimelineAtividadesAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null)
        {
            var query = _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .AsQueryable();

            // Aplicar filtros
            if (usuarioIds?.Any() == true)
            {
                query = query.Where(a => usuarioIds.Contains(a.UsuarioId));
            }

            if (projetoIds?.Any() == true)
            {
                query = query.Where(a => a.ProjetoId.HasValue && projetoIds.Contains(a.ProjetoId.Value));
            }

            var atividades = await query
                .Where(a => a.DataCriacao >= dataInicio && a.DataCriacao <= dataFim)
                .ToListAsync();

            // Gerar eventos da timeline
            var events = new List<TimelineAtividadeViewModel>();

            foreach (var atividade in atividades)
            {
                // Lógica de criação de eventos (similar ao Page Model)
                // ... implementar conforme necessário
            }

            return events.OrderByDescending(e => e.DataReferencia).ToList();
        }
    }
}
