using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Pages.Timeline
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        public IList<TimelineAtividadeViewModel> AtividadesTimeline { get; set; } = new List<TimelineAtividadeViewModel>();
        public IList<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public IList<Projeto> Projetos { get; set; } = new List<Projeto>();

        [BindProperty(SupportsGet = true)]
        public int PeriodoDias { get; set; } = 30;

        [BindProperty(SupportsGet = true)]
        public List<int> UsuariosSelecionados { get; set; } = new List<int>();

        [BindProperty(SupportsGet = true)]
        public List<int> ProjetosSelecionados { get; set; } = new List<int>();

        public async Task OnGetAsync()
        {
            // Carregar dados para filtros
            Usuarios = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            Projetos = await _context.Projetos
                .Where(p => p.Status.Nome != "Cancelado")
                .OrderBy(p => p.Nome)
                .ToListAsync();

            // Definir período
            var dataInicio = DateTime.Today.AddDays(-PeriodoDias);
            var dataFim = DateTime.Today.AddDays(1);

            // Query base das atividades
            var query = _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .Where(a => a.DataCriacao >= dataInicio && a.DataCriacao <= dataFim)
                .AsQueryable();

            // Aplicar filtros de usuário
            if (UsuariosSelecionados.Any())
            {
                query = query.Where(a => UsuariosSelecionados.Contains(a.UsuarioId));
            }

            // Aplicar filtros de projeto
            if (ProjetosSelecionados.Any())
            {
                query = query.Where(a => a.ProjetoId.HasValue && ProjetosSelecionados.Contains(a.ProjetoId.Value));
            }

            var atividades = await query.ToListAsync();

            // Criar timeline events
            var timelineEvents = new List<TimelineAtividadeViewModel>();

            foreach (var atividade in atividades)
            {
                // Evento de criação
                timelineEvents.Add(new TimelineAtividadeViewModel
                {
                    Id = atividade.Id,
                    AtividadeNome = atividade.Nome,
                    Descricao = atividade.Descricao,
                    ProjetoNome = atividade.Projeto?.Nome,
                    UsuarioNome = atividade.Usuario.Nome,
                    TipoAtividadeNome = atividade.TipoAtividade.Nome,
                    StatusNome = atividade.Status.Nome,
                    StatusCor = atividade.Status.Cor,
                    DataReferencia = atividade.DataCriacao,
                    TipoEvento = "Criacao",
                    Prioridade = atividade.Prioridade
                });

                // Evento de início (se diferente da criação)
                if (atividade.DataInicio.Date != atividade.DataCriacao.Date && atividade.DataInicio >= dataInicio)
                {
                    timelineEvents.Add(new TimelineAtividadeViewModel
                    {
                        Id = atividade.Id,
                        AtividadeNome = atividade.Nome,
                        Descricao = atividade.Descricao,
                        ProjetoNome = atividade.Projeto?.Nome,
                        UsuarioNome = atividade.Usuario.Nome,
                        TipoAtividadeNome = atividade.TipoAtividade.Nome,
                        StatusNome = atividade.Status.Nome,
                        StatusCor = atividade.Status.Cor,
                        DataReferencia = atividade.DataInicio,
                        TipoEvento = "Inicio",
                        Prioridade = atividade.Prioridade
                    });
                }

                // Evento de conclusão
                if (atividade.DataFimReal.HasValue && atividade.DataFimReal >= dataInicio)
                {
                    timelineEvents.Add(new TimelineAtividadeViewModel
                    {
                        Id = atividade.Id,
                        AtividadeNome = atividade.Nome,
                        Descricao = atividade.Descricao,
                        ProjetoNome = atividade.Projeto.Nome,
                        UsuarioNome = atividade.Usuario.Nome,
                        TipoAtividadeNome = atividade.TipoAtividade.Nome,
                        StatusNome = atividade.Status.Nome,
                        StatusCor = atividade.Status.Cor,
                        DataReferencia = atividade.DataFimReal.Value,
                        TipoEvento = "Conclusao",
                        Prioridade = atividade.Prioridade
                    });
                }

                // Evento de atualização (se foi atualizada)
                if (atividade.DataAtualizacao != atividade.DataCriacao && atividade.DataAtualizacao >= dataInicio)
                {
                    timelineEvents.Add(new TimelineAtividadeViewModel
                    {
                        Id = atividade.Id,
                        AtividadeNome = atividade.Nome,
                        Descricao = atividade.Descricao,
                        ProjetoNome = atividade.Projeto?.Nome,
                        UsuarioNome = atividade.Usuario.Nome,
                        TipoAtividadeNome = atividade.TipoAtividade.Nome,
                        StatusNome = atividade.Status.Nome,
                        StatusCor = atividade.Status.Cor,
                        DataReferencia = atividade.DataAtualizacao,
                        TipoEvento = "Atualizacao",
                        Prioridade = atividade.Prioridade
                    });
                }
            }

            AtividadesTimeline = timelineEvents.OrderByDescending(t => t.DataReferencia).ToList();
        }
    }
}
