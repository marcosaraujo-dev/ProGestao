using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels;

namespace ProGestao.Pages.Grid
{
    public class IndexModel : PageModel
    {
        private readonly ProGestaoContext _context;

        public IndexModel(ProGestaoContext context)
        {
            _context = context;
        }

        public IList<UsuarioGridViewModel> GridData { get; set; } = new List<UsuarioGridViewModel>();

        [BindProperty(SupportsGet = true)]
        public DateTime DataInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime DataFim { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Definir período padrão (semana atual) se não fornecido
                if (DataInicio == default || DataFim == default)
                {
                    var hoje = DateTime.Today;
                    var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek); // Domingo
                    DataInicio = inicioSemana;
                    DataFim = inicioSemana.AddDays(6); // Sábado
                }

                // Carregar usuários ativos
                var usuarios = await _context.Usuarios
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                // Carregar atividades do período com todos os relacionamentos necessários
                var atividades = await _context.Atividades
                    .Include(a => a.Projeto)
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Status)
                    .Where(a => a.UsuarioId != null && // Garantir que tem usuário
                           ((a.DataInicio.Date >= DataInicio.Date && a.DataInicio.Date <= DataFim.Date) ||
                           (a.DataFimPrevista.HasValue && a.DataFimPrevista.Value.Date >= DataInicio.Date && a.DataFimPrevista.Value.Date <= DataFim.Date) ||
                           (a.DataFimReal.HasValue && a.DataFimReal.Value.Date >= DataInicio.Date && a.DataFimReal.Value.Date <= DataFim.Date) ||
                           (a.DataInicio.Date < DataInicio.Date &&
                            ((a.DataFimReal.HasValue && a.DataFimReal.Value.Date > DataFim.Date) ||
                             (a.DataFimReal == null && (a.DataFimPrevista == null || a.DataFimPrevista.Value.Date > DataFim.Date))))))
                    .ToListAsync();

                // Construir grid data
                var gridData = new List<UsuarioGridViewModel>();

                foreach (var usuario in usuarios)
                {
                    var usuarioGrid = new UsuarioGridViewModel
                    {
                        Id = usuario.Id,
                        Nome = usuario.Nome ?? "Usuário sem nome",
                        Cargo = usuario.Cargo ?? "Sem cargo",
                        EquipeNome = usuario.Equipe?.Nome ?? "Sem equipe",
                        AtividadesPorDia = new Dictionary<DateTime, List<AtividadeGridViewModel>>()
                    };

                    // Inicializar todos os dias do período
                    for (var data = DataInicio.Date; data <= DataFim.Date; data = data.AddDays(1))
                    {
                        usuarioGrid.AtividadesPorDia[data] = new List<AtividadeGridViewModel>();
                    }

                    // Preencher atividades do usuário
                    var atividadesUsuario = atividades.Where(a => a.UsuarioId == usuario.Id);

                    foreach (var atividade in atividadesUsuario)
                    {
                        // Verificações de nulidade para evitar exceções
                        if (atividade == null) continue;

                        var atividadeGrid = new AtividadeGridViewModel
                        {
                            Id = atividade.Id,
                            Nome = atividade.Nome ?? "Atividade sem nome",
                            Descricao = atividade.Descricao ?? "",
                            ProjetoNome = atividade.Projeto?.Nome ?? "Sem projeto",
                            TipoAtividadeNome = atividade.TipoAtividade?.Nome ?? "Sem tipo",
                            StatusNome = atividade.Status?.Nome ?? "Sem status",
                            StatusCor = atividade.Status?.Cor ?? "#6c757d",
                            DataInicio = atividade.DataInicio,
                            DataFimPrevista = atividade.DataFimPrevista,
                            DataFimReal = atividade.DataFimReal,
                            Prioridade = atividade.Prioridade
                        };

                        // Determinar classe CSS baseada no status
                     //   atividadeGrid.StatusClasse = GetStatusClasse(atividade.Status?.Nome);
                     //   atividadeGrid.PrioridadeCor = GetPrioridadeCor(atividade.Prioridade);
                     //   atividadeGrid.PrioridadeTexto = GetPrioridadeTexto(atividade.Prioridade);

                        // Determinar em quais dias a atividade aparece
                        var dataInicioAtividade = atividade.DataInicio.Date;
                        var dataFimAtividade = atividade.DataFimReal?.Date ??
                                              atividade.DataFimPrevista?.Date ??
                                              DataFim.Date;

                        // Garantir que as datas estejam dentro do período
                        dataInicioAtividade = dataInicioAtividade < DataInicio.Date ? DataInicio.Date : dataInicioAtividade;
                        dataFimAtividade = dataFimAtividade > DataFim.Date ? DataFim.Date : dataFimAtividade;

                        // Adicionar a atividade aos dias correspondentes
                        for (var data = dataInicioAtividade; data <= dataFimAtividade; data = data.AddDays(1))
                        {
                            if (usuarioGrid.AtividadesPorDia.ContainsKey(data))
                            {
                                usuarioGrid.AtividadesPorDia[data].Add(atividadeGrid);
                            }
                        }
                    }

                    gridData.Add(usuarioGrid);
                }

                GridData = gridData;
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                Console.WriteLine($"Erro ao carregar grid: {ex.Message}");

                // Inicializar com dados vazios para evitar crash
                GridData = new List<UsuarioGridViewModel>();

                // Opcionalmente, adicionar mensagem de erro ao TempData
                TempData["ErrorMessage"] = "Erro ao carregar o grid de atividades. Tente novamente.";
            }
        }

        private string GetStatusClasse(string? statusNome)
        {
            return statusNome?.ToLower() switch
            {
                "pendente" => "pendente",
                "em andamento" => "andamento",
                "pausada" => "pausada",
                "concluída" or "concluida" => "concluida",
                "cancelada" => "cancelada",
                _ => "pendente"
            };
        }

        private string GetPrioridadeCor(int prioridade)
        {
            return prioridade switch
            {
                1 => "success", // Baixa
                2 => "info",    // Normal
                3 => "warning", // Alta
                4 => "danger",  // Crítica
                _ => "secondary"
            };
        }

        private string GetPrioridadeTexto(int prioridade)
        {
            return prioridade switch
            {
                1 => "Baixa",
                2 => "Normal",
                3 => "Alta",
                4 => "Crítica",
                _ => "Normal"
            };
        }
    }
}