using Microsoft.EntityFrameworkCore;
using ProGestao.Models;

namespace ProGestao.Data
{
    public class ProGestaoContext : DbContext
    {
        public ProGestaoContext(DbContextOptions<ProGestaoContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Equipe> Equipes { get; set; }
        public DbSet<Projeto> Projetos { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<StatusProjeto> StatusProjetos { get; set; }
        public DbSet<StatusAtividade> StatusAtividades { get; set; }
        public DbSet<TipoAtividade> TiposAtividade { get; set; }

     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações de relacionamentos
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Equipe)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EquipeId);

            modelBuilder.Entity<Projeto>()
                .HasOne(p => p.Responsavel)
                .WithMany(u => u.ProjetosResponsavel)
                .HasForeignKey(p => p.ResponsavelId);

            modelBuilder.Entity<Atividade>()
                .HasOne(a => a.Projeto)
                .WithMany(p => p.Atividades)
                .HasForeignKey(a => a.ProjetoId);

            modelBuilder.Entity<Atividade>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Atividades)
                .HasForeignKey(a => a.UsuarioId);

            // Precisão para campos decimais
            modelBuilder.Entity<Atividade>()
                .Property(a => a.HorasEstimadas)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Atividade>()
                .Property(a => a.HorasReais)
                .HasPrecision(5, 2);

            modelBuilder.Entity<StatusProjeto>().ToTable("StatusProjeto");
            modelBuilder.Entity<StatusAtividade>().ToTable("StatusAtividade");

            base.OnModelCreating(modelBuilder);
        }
    }
}
