using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        public IList<UsuarioViewModel> Usuarios { get; set; } = new List<UsuarioViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? FiltroEquipe { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroCargo { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FiltroAtivo { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? FiltroStatus { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? FiltroResponsavel { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Usuarios
                .Include(u => u.Equipe)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(FiltroEquipe) && int.TryParse(FiltroEquipe, out var equipeId))
            {
                query = query.Where(u => u.EquipeId == equipeId);
            }

            if (!string.IsNullOrEmpty(FiltroCargo))
            {
                query = query.Where(u => u.Cargo.Contains(FiltroCargo));
            }

            if (FiltroAtivo.HasValue)
            {
                query = query.Where(u => u.Ativo == FiltroAtivo.Value);
            }

            var usuarios = await query.OrderBy(u => u.Nome).ToListAsync();

            Usuarios = new List<UsuarioViewModel>();

            foreach (var usuario in usuarios)
            {
                var totalAtividades = await _context.Atividades
                    .Where(a => a.UsuarioId == usuario.Id)
                    .CountAsync();

                var atividadesAndamento = await _context.Atividades
                    .Where(a => a.UsuarioId == usuario.Id && a.Status.Nome == "Em Andamento")
                    .CountAsync();

                var atividadesConcluidas = await _context.Atividades
                    .Where(a => a.UsuarioId == usuario.Id && a.Status.Nome == "Conclu�da")
                    .CountAsync();

                Usuarios.Add(new UsuarioViewModel
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Cargo = usuario.Cargo,
                    EquipeId = usuario.EquipeId,
                    EquipeNome = usuario.Equipe.Nome,
                    DataAdmissao = usuario.DataAdmissao,
                    Ativo = usuario.Ativo,
                    TotalAtividades = totalAtividades,
                    AtividadesAndamento = atividadesAndamento,
                    AtividadesConcluidas = atividadesConcluidas,
                    DataCriacao = usuario.DataCriacao
                });
            }
        }
    }
}