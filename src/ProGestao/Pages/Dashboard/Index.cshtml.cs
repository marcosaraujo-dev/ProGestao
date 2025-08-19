using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels;

namespace ProGestao.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        // Métricas Principais
        public int TotalAtividades { get; set; }
        public int AtividadesAndamento { get; set; }
        public int AtividadesPendentes { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesCanceladas { get; set; }
        public int AtividadesAtrasadas { get; set; }
        public int ProjetosAtivos { get; set; }
        public int UsuariosAtivos { get; set; }

        // Métricas por Prioridade
        public int AtividadesPrioridadeBaixa { get; set; }
        public int AtividadesPrioridadeNormal { get; set; }
        public int AtividadesPrioridadeAlta { get; set; }
        public int AtividadesPrioridadeCritica { get; set; }

        // Listas para Widgets
        public IList<AtividadeViewModel> AtividadesRecentes { get; set; } = new List<AtividadeViewModel>();
        public IList<AtividadeViewModel> ProximosVencimentos { get; set; } = new List<AtividadeViewModel>();
        public IList<PerformanceUsuarioViewModel> PerformanceEquipe { get; set; } = new List<PerformanceUsuarioViewModel>();

        public async Task OnGetAsync()
        {
            var hoje = DateTime.Today;
            var proximaSemanaDt = hoje.AddDays(7);

            // Métricas Principais
            TotalAtividades = await _context.Atividades.CountAsync();

            AtividadesAndamento = await _context.Atividades
                .Where(a => a.Status.Nome == "Em Andamento")
                .CountAsync();

            AtividadesPendentes = await _context.Atividades
                .Where(a => a.Status.Nome == "Pendente")
                .CountAsync();

            AtividadesConcluidas = await _context.Atividades
                .Where(a => a.Status.Nome == "Concluída")
                .CountAsync();

            AtividadesCanceladas = await _context.Atividades
                .Where(a => a.Status.Nome == "Cancelada")
                .CountAsync();

            AtividadesAtrasadas = await _context.Atividades
                .Where(a => a.DataFimPrevista < hoje && a.DataFimReal == null && a.Status.Nome != "Concluída")
                .CountAsync();

            ProjetosAtivos = await _context.Projetos
                .Where(p => p.Status.Nome != "Concluído" && p.Status.Nome != "Cancelado")
                .CountAsync();

            UsuariosAtivos = await _context.Usuarios
                .Where(u => u.Ativo)
                .CountAsync();

            // Métricas por Prioridade
            AtividadesPrioridadeBaixa = await _context.Atividades.Where(a => a.Prioridade == 1).CountAsync();
            AtividadesPrioridadeNormal = await _context.Atividades.Where(a => a.Prioridade == 2).CountAsync();
            AtividadesPrioridadeAlta = await _context.Atividades.Where(a => a.Prioridade == 3).CountAsync();
            AtividadesPrioridadeCritica = await _context.Atividades.Where(a => a.Prioridade == 4).CountAsync();

            // Atividades Recentes (últimas 10)
            var atividadesRecentes = await _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.Status)
                .OrderByDescending(a => a.DataCriacao)
                .Take(10)
                .ToListAsync();

            AtividadesRecentes = atividadesRecentes.Select(MapToViewModel).ToList();

            // Próximos Vencimentos (próximas 2 semanas)
            var proximosVencimentos = await _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.Status)
                .Where(a => a.DataFimPrevista <= proximaSemanaDt &&
                           a.DataFimReal == null &&
                           a.Status.Nome != "Concluída")
                .OrderBy(a => a.DataFimPrevista)
                .Take(10)
                .ToListAsync();

            ProximosVencimentos = proximosVencimentos.Select(MapToViewModel).ToList();

            // Performance da Equipe
            var usuarios = await _context.Usuarios
                .Where(u => u.Ativo)
                .ToListAsync();

            var performanceList = new List<PerformanceUsuarioViewModel>();

            foreach (var usuario in usuarios)
            {
                var totalAtividades = await _context.Atividades
                    .Where(a => a.UsuarioId == usuario.Id)
                    .CountAsync();

                if (totalAtividades > 0)
                {
                    var concluidas = await _context.Atividades
                        .Where(a => a.UsuarioId == usuario.Id && a.Status.Nome == "Concluída")
                        .CountAsync();

                    var emAndamento = await _context.Atividades
                        .Where(a => a.UsuarioId == usuario.Id && a.Status.Nome == "Em Andamento")
                        .CountAsync();

                    var atrasadas = await _context.Atividades
                        .Where(a => a.UsuarioId == usuario.Id &&
                                   a.DataFimPrevista < hoje &&
                                   a.DataFimReal == null &&
                                   a.Status.Nome != "Concluída")
                        .CountAsync();

                    performanceList.Add(new PerformanceUsuarioViewModel
                    {
                        UsuarioId = usuario.Id,
                        NomeUsuario = usuario.Nome,
                        Cargo = usuario.Cargo,
                        TotalAtividades = totalAtividades,
                        AtividadesConcluidas = concluidas,
                        AtividadesAndamento = emAndamento,
                        AtividadesAtrasadas = atrasadas,
                        TaxaConclusao = totalAtividades > 0 ? (decimal)concluidas / totalAtividades : 0
                    });
                }
            }

            PerformanceEquipe = performanceList.OrderByDescending(p => p.TaxaConclusao).ToList();
        }

        private static AtividadeViewModel MapToViewModel(Models.Atividade atividade)
        {
            return new AtividadeViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                Descricao = atividade.Descricao,
                ProjetoId = atividade.ProjetoId,
                ProjetoNome = atividade.Projeto?.Nome,
                UsuarioId = atividade.UsuarioId,
                UsuarioNome = atividade.Usuario.Nome,
                StatusId = atividade.StatusId,
                StatusNome = atividade.Status.Nome,
                StatusCor = atividade.Status.Cor,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimPrevista,
                DataFimReal = atividade.DataFimReal,
                Prioridade = atividade.Prioridade,
                DataCriacao = atividade.DataCriacao
            };
        }
    }
}
