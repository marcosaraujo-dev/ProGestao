using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Pages.Atividades
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        public IList<AtividadeViewModel> Atividades { get; set; } = new List<AtividadeViewModel>();
        public IList<Projeto> Projetos { get; set; } = new List<Projeto>();
        public IList<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public IList<StatusAtividade> StatusAtividades { get; set; } = new List<StatusAtividade>();

        [BindProperty(SupportsGet = true)]
        public string? FiltroProjetoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroUsuarioId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroStatusId { get; set; }

        public async Task OnGetAsync()
        {
            // Carregar dados para os filtros
            Projetos = await _context.Projetos
                .Where(p => p.Status.Nome != "Cancelado")
                .OrderBy(p => p.Nome)
                .ToListAsync();

            Usuarios = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            StatusAtividades = await _context.StatusAtividades
                .OrderBy(s => s.Ordem)
                .ToListAsync();

            // Query base das atividades
            var query = _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(FiltroProjetoId) && int.TryParse(FiltroProjetoId, out var projetoId))
            {
                if (projetoId == 0)
                {
                    query = query.Where(a => a.ProjetoId == null);
                }
                else
                {
                    query = query.Where(a => a.ProjetoId == projetoId);
                }
            }

            if (!string.IsNullOrEmpty(FiltroUsuarioId) && int.TryParse(FiltroUsuarioId, out var usuarioId))
            {
                query = query.Where(a => a.UsuarioId == usuarioId);
            }

            if (!string.IsNullOrEmpty(FiltroStatusId) && int.TryParse(FiltroStatusId, out var statusId))
            {
                query = query.Where(a => a.StatusId == statusId);
            }

            // Executar query e mapear para ViewModel
            var atividadesList = await query
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();

            Atividades = atividadesList.Select(a => new AtividadeViewModel
            {
                Id = a.Id,
                Nome = a.Nome,
                Descricao = a.Descricao,
                ProjetoId = a.ProjetoId,
                ProjetoNome = a.Projeto != null ? a.Projeto.Nome : string.Empty,
                TipoAtividadeId = a.TipoAtividadeId,
                TipoAtividadeNome = a.TipoAtividade != null ? a.TipoAtividade.Nome : string.Empty,
                StatusId = a.StatusId,
                StatusNome = a.Status != null ? a.Status.Nome : string.Empty,
                StatusCor = a.Status != null ? a.Status.Cor : string.Empty,
                UsuarioId = a.UsuarioId,
                UsuarioNome = a.Usuario != null ? a.Usuario.Nome : string.Empty,
                DataInicio = a.DataInicio,
                DataFimPrevista = a.DataFimPrevista,
                DataFimReal = a.DataFimReal,
                HorasEstimadas = a.HorasEstimadas,
                HorasReais = a.HorasReais,
                Prioridade = a.Prioridade,
                Observacoes = a.Observacoes
            }).ToList();
        }
    }
}
