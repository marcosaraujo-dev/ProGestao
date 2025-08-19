using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels;

namespace ProGestao.Pages.Atividades
{
    public class CreateModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public CreateModel(ProGestaoContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AtividadeViewModel Atividade { get; set; } = new AtividadeViewModel();

        public IList<Projeto> Projetos { get; set; } = new List<Projeto>();
        public IList<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public IList<TipoAtividade> TiposAtividade { get; set; } = new List<TipoAtividade>();
        public IList<StatusAtividade> StatusAtividades { get; set; } = new List<StatusAtividade>();

        public async Task OnGetAsync()
        {
            
            await CarregarDados();

            // Definir valores padrão
            Atividade.DataInicio = DateTime.Now;
            Atividade.Prioridade = 3; // Alta
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarDados();
                return Page();
            }

            var atividade = new Atividade
            {
                Nome = Atividade.Nome,
                Descricao = Atividade.Descricao,
                ProjetoId = Atividade.ProjetoId, 
                TipoAtividadeId = Atividade.TipoAtividadeId,
                StatusId = Atividade.StatusId,
                UsuarioId = Atividade.UsuarioId,
                DataInicio = Atividade.DataInicio,
                DataFimPrevista = Atividade.DataFimPrevista,
                DataFimReal = Atividade.DataFimReal,
                HorasEstimadas = Atividade.HorasEstimadas,
                HorasReais = Atividade.HorasReais,
                Prioridade = Atividade.Prioridade,
                Observacoes = Atividade.Observacoes,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Atividade '{atividade.Nome}' criada com sucesso!";
            return RedirectToPage("./Index");
        }

        private async Task CarregarDados()
        {
            Projetos = await _context.Projetos
                .Where(p => p.Status.Nome != "Cancelado")
                .OrderBy(p => p.Nome)
                .ToListAsync();

            Usuarios = await _context.Usuarios
                .Include(u => u.Equipe)
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            TiposAtividade = await _context.TiposAtividade
                .OrderBy(t => t.Nome)
                .ToListAsync();

            StatusAtividades = await _context.StatusAtividades
                .OrderBy(s => s.Ordem)
                .ToListAsync();
        }
    }
}