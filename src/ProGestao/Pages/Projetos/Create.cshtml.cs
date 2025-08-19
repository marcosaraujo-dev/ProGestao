using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Pages.Projetos
{
    public class CreateModel : PageModel
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ProGestaoContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Projeto Projeto { get; set; } = new Projeto();

        // SelectLists para dropdowns
        public SelectList StatusSelectList { get; set; } = default!;
        public SelectList ResponsaveisSelectList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando página de criação de projeto");

                // Definir valores padrão
                Projeto.DataInicio = DateTime.Today;

                await CarregarSelectLists();

                // Definir status padrão como "Planejamento"
                var statusPlanejamento = await _context.StatusProjetos
                    .FirstOrDefaultAsync(s => s.Nome == "Planejamento");

                if (statusPlanejamento != null)
                {
                    Projeto.StatusId = statusPlanejamento.Id;
                }

                _logger.LogInformation("Página de criação de projeto carregada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de criação de projeto");
                TempData["ErrorMessage"] = "Erro interno do servidor. Tente novamente.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido ao tentar criar projeto");
                    await CarregarSelectLists();
                    return Page();
                }

                _logger.LogInformation("Iniciando criação do projeto '{ProjetoNome}'", Projeto.Nome);

                // Validações de negócio
                var validationResult = await ValidarCriacao(Projeto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validação falhou para novo projeto: {Erro}", validationResult.ErrorMessage);
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    await CarregarSelectLists();
                    return Page();
                }

                // Definir dados do sistema
                Projeto.DataCriacao = DateTime.Now;

                // Adicionar o projeto
                _context.Projetos.Add(Projeto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Projeto '{ProjetoNome}' (ID: {ProjetoId}) criado com sucesso",
                    Projeto.Nome, Projeto.Id);

                TempData["SuccessMessage"] = $"Projeto '{Projeto.Nome}' criado com sucesso!";
                return RedirectToPage("./Details", new { id = Projeto.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao criar projeto");

                // Verificar se é violação de constraint de nome único
                if (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                    ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    ModelState.AddModelError("Projeto.Nome", "Já existe um projeto com este nome.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erro ao salvar o projeto. Verifique os dados e tente novamente.");
                }

                await CarregarSelectLists();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar projeto '{ProjetoNome}'", Projeto?.Nome ?? "Unknown");
                ModelState.AddModelError(string.Empty, "Erro interno do servidor. Tente novamente.");
                await CarregarSelectLists();
                return Page();
            }
        }

        /// <summary>
        /// Valida as regras de negócio para criação do projeto
        /// </summary>
        /// <param name="projeto">Projeto a ser validado</param>
        /// <returns>Resultado da validação</returns>
        private async Task<(bool IsValid, string ErrorMessage)> ValidarCriacao(Projeto projeto)
        {
            try
            {
                // Validar se as datas são consistentes
                if (projeto.DataFimPrevista.HasValue && projeto.DataFimPrevista.Value < projeto.DataInicio)
                {
                    return (false, "A data de fim prevista não pode ser anterior à data de início.");
                }

                // Validar se a data de início não é muito no passado (mais de 1 ano)
                if (projeto.DataInicio < DateTime.Now.AddYears(-1))
                {
                    return (false, "A data de início não pode ser superior a 1 ano no passado.");
                }

                // Validar se o responsável existe (se informado)
                if (projeto.ResponsavelId != 0)
                {
                    var responsavelExists = await _context.Usuarios
                        .AnyAsync(u => u.Id == projeto.ResponsavelId && u.Ativo);

                    if (!responsavelExists)
                    {
                        return (false, "Responsável selecionado não encontrado ou está inativo.");
                    }
                }

                // Validar se o status existe
                if (projeto.StatusId != 0)
                {
                    var statusExists = await _context.StatusProjetos.AnyAsync(s => s.Id == projeto.StatusId);
                    if (!statusExists)
                    {
                        return (false, "Status selecionado não encontrado.");
                    }
                }

                // Validar se já existe um projeto com o mesmo nome
                var nomeJaExiste = await _context.Projetos
                    .AnyAsync(p => p.Nome.ToLower() == projeto.Nome.ToLower());

                if (nomeJaExiste)
                {
                    return (false, "Já existe um projeto com este nome.");
                }

                // Validar URL se fornecida
                if (!string.IsNullOrEmpty(projeto.LinkProjeto))
                {
                    if (!Uri.TryCreate(projeto.LinkProjeto, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        return (false, "Link do projeto deve ser uma URL válida (http:// ou https://).");
                    }
                }

                // Validar tamanho dos campos texto
                if (!string.IsNullOrEmpty(projeto.Nome) && projeto.Nome.Length > 200)
                {
                    return (false, "O nome do projeto não pode ter mais de 200 caracteres.");
                }

                if (!string.IsNullOrEmpty(projeto.Descricao) && projeto.Descricao.Length > 1000)
                {
                    return (false, "A descrição não pode ter mais de 1000 caracteres.");
                }

                if (!string.IsNullOrEmpty(projeto.Observacoes) && projeto.Observacoes.Length > 500)
                {
                    return (false, "As observações não podem ter mais de 500 caracteres.");
                }

                // Validar se o nome do projeto não contém caracteres especiais problemáticos
                var caracteresProibidos = new[] { '<', '>', '"', '\'', '&' };
                if (projeto.Nome.Any(c => caracteresProibidos.Contains(c)))
                {
                    return (false, "O nome do projeto não pode conter os caracteres: < > \" ' &");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação de criação do projeto");
                return (false, "Erro durante validação. Tente novamente.");
            }
        }

        /// <summary>
        /// Carrega as listas de seleção para os dropdowns
        /// </summary>
        private async Task CarregarSelectLists()
        {
            try
            {
                // Carregar status de projetos
                var statusList = await _context.StatusProjetos
                    .OrderBy(s => s.Ordem)
                    .ThenBy(s => s.Nome)
                    .Select(s => new { s.Id, s.Nome })
                    .ToListAsync();

                StatusSelectList = new SelectList(statusList, "Id", "Nome", Projeto?.StatusId);

                // Carregar usuários ativos (responsáveis)
                var responsaveisList = await _context.Usuarios
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .Select(u => new { u.Id, DisplayName = u.Nome + (!string.IsNullOrEmpty(u.Cargo) ? " - " + u.Cargo : "") })
                    .ToListAsync();

                ResponsaveisSelectList = new SelectList(responsaveisList, "Id", "DisplayName", Projeto?.ResponsavelId);

                _logger.LogDebug("SelectLists carregadas: {StatusCount} status, {ResponsaveisCount} responsáveis",
                    statusList.Count, responsaveisList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar SelectLists");

                // Criar listas vazias em caso de erro
                StatusSelectList = new SelectList(new List<object>(), "Id", "Nome");
                ResponsaveisSelectList = new SelectList(new List<object>(), "Id", "DisplayName");

                throw;
            }
        }

        /// <summary>
        /// Método auxiliar para obter sugestões de nomes baseados em projetos existentes
        /// </summary>
        /// <param name="termo">Termo de busca</param>
        /// <returns>Lista de sugestões</returns>
        public async Task<IList<string>> ObterSugestoesNome(string termo)
        {
            try
            {
                if (string.IsNullOrEmpty(termo) || termo.Length < 2)
                {
                    return new List<string>();
                }

                var sugestoes = await _context.Projetos
                    .Where(p => p.Nome.Contains(termo))
                    .Select(p => p.Nome)
                    .Distinct()
                    .Take(5)
                    .ToListAsync();

                return sugestoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sugestões de nome para termo '{Termo}'", termo);
                return new List<string>();
            }
        }

        /// <summary>
        /// Verifica se um nome de projeto está disponível
        /// </summary>
        /// <param name="nome">Nome a verificar</param>
        /// <returns>True se disponível, False se já existe</returns>
        public async Task<bool> NomeDisponivel(string nome)
        {
            try
            {
                if (string.IsNullOrEmpty(nome))
                {
                    return false;
                }

                return !await _context.Projetos
                    .AnyAsync(p => p.Nome.ToLower() == nome.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar disponibilidade do nome '{Nome}'", nome);
                return false;
            }
        }
    }
}