using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Usuarios
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
        public UsuarioViewModel Usuario { get; set; } = new UsuarioViewModel();

        public IList<Equipe> Equipes { get; set; } = new List<Equipe>();

        public async Task OnGetAsync()
        {
            await CarregarDados();

            // Definir valores padrão
            Usuario.Ativo = true;
            Usuario.DataAdmissao = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validações customizadas
            await ValidarDadosCustomizados();

            if (!ModelState.IsValid)
            {
                await CarregarDados();
                return Page();
            }

            try
            {
                // Verificar se email já existe
                var emailExistente = await _context.Usuarios
                    .AnyAsync(u => u.Email.ToLower() == Usuario.Email.ToLower());

                if (emailExistente)
                {
                    ModelState.AddModelError("Usuario.Email", "Este email já está em uso por outro usuário.");
                    await CarregarDados();
                    return Page();
                }

                // Criar novo usuário
                var usuario = new Usuario
                {
                    Nome = Usuario.Nome.Trim(),
                    Email = Usuario.Email.Trim().ToLower(),
                    Cargo = Usuario.Cargo.Trim(),
                    EquipeId = Usuario.EquipeId,
                    DataAdmissao = Usuario.DataAdmissao,
                    Ativo = Usuario.Ativo,
                    DataCriacao = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário criado com sucesso: {Nome} ({Email})", usuario.Nome, usuario.Email);

                TempData["SuccessMessage"] = $"Usuário '{usuario.Nome}' criado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário: {Nome} ({Email})", Usuario.Nome, Usuario.Email);

                ModelState.AddModelError("", "Ocorreu um erro ao salvar o usuário. Tente novamente.");
                await CarregarDados();
                return Page();
            }
        }

        private async Task CarregarDados()
        {
            try
            {
                Equipes = await _context.Equipes
                    .Where(e => e.Ativo)
                    .OrderBy(e => e.Nome)
                    .ToListAsync();

                if (!Equipes.Any())
                {
                    ModelState.AddModelError("", "Nenhuma equipe cadastrada. Cadastre pelo menos uma equipe antes de criar usuários.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para criação de usuário");
                ModelState.AddModelError("", "Erro ao carregar dados necessários.");
            }
        }

        private async Task ValidarDadosCustomizados()
        {
            // Validar se equipe existe e está ativa
            if (Usuario.EquipeId > 0)
            {
                var equipeExiste = await _context.Equipes
                    .AnyAsync(e => e.Id == Usuario.EquipeId && e.Ativo);

                if (!equipeExiste)
                {
                    ModelState.AddModelError("Usuario.EquipeId", "Equipe selecionada não existe ou está inativa.");
                }
            }

            // Validar data de admissão
            if (Usuario.DataAdmissao.HasValue)
            {
                if (Usuario.DataAdmissao.Value > DateTime.Today)
                {
                    ModelState.AddModelError("Usuario.DataAdmissao", "Data de admissão não pode ser no futuro.");
                }

                if (Usuario.DataAdmissao.Value < new DateTime(1950, 1, 1))
                {
                    ModelState.AddModelError("Usuario.DataAdmissao", "Data de admissão inválida.");
                }
            }

            // Validar formato do email
            if (!string.IsNullOrEmpty(Usuario.Email))
            {
                try
                {
                    var email = new System.Net.Mail.MailAddress(Usuario.Email);
                    if (email.Address != Usuario.Email.Trim())
                    {
                        ModelState.AddModelError("Usuario.Email", "Formato de email inválido.");
                    }
                }
                catch
                {
                    ModelState.AddModelError("Usuario.Email", "Formato de email inválido.");
                }
            }

            // Validar nome (não pode ter apenas números ou caracteres especiais)
            if (!string.IsNullOrEmpty(Usuario.Nome))
            {
                if (Usuario.Nome.Trim().Length < 2)
                {
                    ModelState.AddModelError("Usuario.Nome", "Nome deve ter pelo menos 2 caracteres.");
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(Usuario.Nome, @"^\d+$"))
                {
                    ModelState.AddModelError("Usuario.Nome", "Nome não pode conter apenas números.");
                }
            }

            // Validar cargo
            if (!string.IsNullOrEmpty(Usuario.Cargo))
            {
                if (Usuario.Cargo.Trim().Length < 2)
                {
                    ModelState.AddModelError("Usuario.Cargo", "Cargo deve ter pelo menos 2 caracteres.");
                }
            }
        }
    }
}