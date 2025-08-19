using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels;

namespace ProGestao.Pages.Projetos
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        public IList<ProjetoViewModel> Projetos { get; set; } = new List<ProjetoViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? FiltroStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroResponsavel { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Projetos
                .Include(p => p.Status)
                .Include(p => p.Responsavel)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(FiltroStatus))
            {
                query = query.Where(p => p.Status.Nome.Contains(FiltroStatus));
            }

            if (!string.IsNullOrEmpty(FiltroResponsavel) && int.TryParse(FiltroResponsavel, out var responsavelId))
            {
                query = query.Where(p => p.ResponsavelId == responsavelId);
            }

            var projetos = await query.OrderByDescending(p => p.DataCriacao).ToListAsync();

            Projetos = new List<ProjetoViewModel>();

            foreach (var projeto in projetos)
            {
                var totalAtividades = await _context.Atividades
                    .Where(a => a.ProjetoId == projeto.Id)
                    .CountAsync();

                var atividadesConcluidas = await _context.Atividades
                    .Where(a => a.ProjetoId == projeto.Id && a.Status.Nome == "Concluída")
                    .CountAsync();

                Projetos.Add(new ProjetoViewModel
                {
                    Id = projeto.Id,
                    Nome = projeto.Nome,
                    Descricao = projeto.Descricao,
                    DataInicio = projeto.DataInicio,
                    DataFimPrevista = projeto.DataFimPrevista,
                    DataFimReal = projeto.DataFimReal,
                    StatusId = projeto.StatusId,
                    StatusNome = projeto.Status.Nome,
                    StatusCor = projeto.Status.Cor,
                    ResponsavelId = projeto.ResponsavelId,
                    ResponsavelNome = projeto.Responsavel?.Nome,
                    LinkProjeto = projeto.LinkProjeto,
                    Observacoes = projeto.Observacoes,
                    TotalAtividades = totalAtividades,
                    AtividadesConcluidas = atividadesConcluidas,
                    DataCriacao = projeto.DataCriacao
                });
            }
        }
    }
}
