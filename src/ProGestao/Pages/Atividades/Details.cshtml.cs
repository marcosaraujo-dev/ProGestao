using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Pages.Atividades
{
    public class DetailsModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public DetailsModel(ProGestaoContext context)
        {
            _context = context;
        }

        public Atividade Atividade { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                    .ThenInclude(u => u.Equipe)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atividade == null)
            {
                return NotFound();
            }

            Atividade = atividade;
            return Page();
        }
    }
}