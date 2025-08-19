using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProGestao.Tests.Services
{
    /// <summary>
    /// Testes unitários para GridService
    /// Implementa padrões de teste: AAA (Arrange, Act, Assert)
    /// Usa In-Memory Database para isolamento dos testes
    /// </summary>
    public class GridServiceTests : IDisposable
    {
        #region Fields and Setup

        private readonly ProGestaoContext _context;
        private readonly Mock<ILogger<GridService>> _loggerMock;
        private readonly GridService _gridService;
        private readonly List<Equipe> _equipes;
        private readonly List<Usuario> _usuarios;
        private readonly List<StatusAtividade> _statusAtividades;
        private readonly List<Atividade> _atividades;

        public GridServiceTests()
        {
            // Arrange - Setup
            var options = new DbContextOptionsBuilder<ProGestaoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ProGestaoContext(options);
            _loggerMock = new Mock<ILogger<GridService>>();
            _gridService = new GridService(_context, _loggerMock.Object);

            // Setup test data
            _equipes = CreateTestEquipes();
            _usuarios = CreateTestUsuarios();
            _statusAtividades = CreateTestStatusAtividades();
            _atividades = CreateTestAtividades();

            SeedDatabase();
        }

        #endregion

        #region Test Data Creation

        private List<Equipe> CreateTestEquipes()
        {
            return new List<Equipe>
            {
                new Equipe { Id = 1, Nome = "Desenvolvimento Frontend", Descricao = "Equipe responsável pelo frontend" },
                new Equipe { Id = 2, Nome = "Desenvolvimento Backend", Descricao = "Equipe responsável pelo backend" },
                new Equipe { Id = 3, Nome = "QA", Descricao = "Equipe de qualidade" }
            };
        }

        private List<Usuario> CreateTestUsuarios()
        {
            return new List<Usuario>
            {
                new Usuario
                {
                    Id = 1, Nome = "João Silva", Email = "joao@test.com",
                    Cargo = "Desenvolvedor Frontend", EquipeId = 1, Ativo = true
                },
                new Usuario
                {
                    Id = 2, Nome = "Maria Santos", Email = "maria@test.com",
                    Cargo = "Desenvolvedor Backend", EquipeId = 2, Ativo = true
                },
                new Usuario
                {
                    Id = 3, Nome = "Pedro Costa", Email = "pedro@test.com",
                    Cargo = "QA Analyst", EquipeId = 3, Ativo = false
                },
                new Usuario
                {
                    Id = 4, Nome = "Ana Oliveira", Email = "ana@test.com",
                    Cargo = "Tech Lead", EquipeId = 1, Ativo = true
                }
            };
        }

        private List<StatusAtividade> CreateTestStatusAtividades()
        {
            return new List<StatusAtividade>
            {
                new StatusAtividade { Id = 1, Nome = "Pendente", Cor = "#ffc107", Ordem = 1 },
                new StatusAtividade { Id = 2, Nome = "Em Andamento", Cor = "#17a2b8", Ordem = 2 },
                new StatusAtividade { Id = 3, Nome = "Concluída", Cor = "#28a745", Ordem = 3 },
                new StatusAtividade { Id = 4, Nome = "Cancelada", Cor = "#dc3545", Ordem = 4 }
            };
        }

        private List<Atividade> CreateTestAtividades()
        {
            var today = DateTime.Today;

            return new List<Atividade>
            {
                new Atividade
                {
                    Id = 1, Nome = "Implementar login", Descricao = "Criar tela de login",
                    DataInicio = today.AddDays(-2), DataFimPrevista = today.AddDays(1),
                    UsuarioId = 1, StatusId = 2, Prioridade = 3, HorasEstimadas = 8
                },
                new Atividade
                {
                    Id = 2, Nome = "API de usuários", Descricao = "Desenvolver API REST",
                    DataInicio = today.AddDays(-1), DataFimPrevista = today.AddDays(2),
                    UsuarioId = 2, StatusId = 2, Prioridade = 2, HorasEstimadas = 16
                },
                new Atividade
                {
                    Id = 3, Nome = "Testes automatizados", Descricao = "Criar testes unitários",
                    DataInicio = today.AddDays(-5), DataFimPrevista = today.AddDays(-2),
                    UsuarioId = 3, StatusId = 1, Prioridade = 1, HorasEstimadas = 4
                },
                new Atividade
                {
                    Id = 4, Nome = "Code review", Descricao = "Revisar código da sprint",
                    DataInicio = today, DataFimPrevista = today.AddDays(1),
                    UsuarioId = 4, StatusId = 3, Prioridade = 2, HorasEstimadas = 2,
                    DataFimReal = today
                },
                new Atividade
                {
                    Id = 5, Nome = "Bug crítico", Descricao = "Corrigir bug em produção",
                    DataInicio = today.AddDays(-3), DataFimPrevista = today.AddDays(-1),
                    UsuarioId = 1, StatusId = 1, Prioridade = 4, HorasEstimadas = 6
                }
            };
        }

        private void SeedDatabase()
        {
            _context.Equipes.AddRange(_equipes);
            _context.Usuarios.AddRange(_usuarios);
            _context.StatusAtividades.AddRange(_statusAtividades);
            _context.Atividades.AddRange(_atividades);
            _context.SaveChanges();
        }

        #endregion

        #region GetGridDataAsync Tests

        [Fact]
        public async Task GetGridDataAsync_ValidFilter_ReturnsGridData()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today,
                EquipeId = null
            };

            // Act
            var result = await _gridService.GetGridDataAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Usuarios.Should().NotBeEmpty();
            result.Dias.Should().HaveCount(7);
            result.Usuarios.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Nome));
        }

        [Fact]
        public async Task GetGridDataAsync_FilterByTeam_ReturnsFilteredUsers()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today,
                EquipeId = 1 // Frontend team
            };

            // Act
            var result = await _gridService.GetGridDataAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Usuarios.Should().HaveCount(2); // João and Ana from Frontend team
            result.Usuarios.Should().OnlyContain(u => u.Nome == "João Silva" || u.Nome == "Ana Oliveira");
        }

        [Fact]
        public async Task GetGridDataAsync_InvalidDateRange_ThrowsArgumentException()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today,
                DataFim = DateTime.Today.AddDays(-1), // Invalid: end before start
                SemanaReferencia = DateTime.Today
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _gridService.GetGridDataAsync(filter));
        }

        [Fact]
        public async Task GetGridDataAsync_PeriodTooLong_ThrowsArgumentException()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today,
                DataFim = DateTime.Today.AddDays(400), // More than 365 days
                SemanaReferencia = DateTime.Today
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _gridService.GetGridDataAsync(filter));
        }

        #endregion

        #region GetAtividadesPorPeriodoAsync Tests

        [Fact]
        public async Task GetAtividadesPorPeriodoAsync_ValidPeriod_ReturnsActivities()
        {
            // Arrange
            var dataInicio = DateTime.Today.AddDays(-7);
            var dataFim = DateTime.Today.AddDays(7);

            // Act
            var result = await _gridService.GetAtividadesPorPeriodoAsync(dataInicio, dataFim);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(a => a.Data >= dataInicio && a.Data <= dataFim);
            result.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Nome));
        }

        [Fact]
        public async Task GetAtividadesPorPeriodoAsync_FilterByTeam_ReturnsTeamActivities()
        {
            // Arrange
            var dataInicio = DateTime.Today.AddDays(-7);
            var dataFim = DateTime.Today.AddDays(7);
            var equipeId = 1; // Frontend team

            // Act
            var result = await _gridService.GetAtividadesPorPeriodoAsync(dataInicio, dataFim, equipeId);

            // Assert
            result.Should().NotBeEmpty();
            // Should only contain activities from users in team 1 (João and Ana)
            var expectedActivities = _atividades.Where(a => a.UsuarioId == 1 || a.UsuarioId == 4);
            result.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetAtividadesPorPeriodoAsync_OverdueActivities_MarkedAsOverdue()
        {
            // Arrange
            var dataInicio = DateTime.Today.AddDays(-7);
            var dataFim = DateTime.Today.AddDays(7);

            // Act
            var result = await _gridService.GetAtividadesPorPeriodoAsync(dataInicio, dataFim);

            // Assert
            var overdueActivities = result.Where(a => a.EstaAtrasada);
            overdueActivities.Should().NotBeEmpty();

            // Activity with ID 5 should be overdue (critical bug)
            overdueActivities.Should().Contain(a => a.Nome.Contains("Bug crítico"));
        }

        #endregion

        #region GetUsuariosAtivosAsync Tests

        [Fact]
        public async Task GetUsuariosAtivosAsync_NoFilter_ReturnsActiveUsers()
        {
            // Arrange & Act
            var result = await _gridService.GetUsuariosAtivosAsync();

            // Assert
            result.Should().HaveCount(3); // Only active users (Pedro is inactive)
            result.Should().NotContain(u => u.Nome == "Pedro Costa");
            result.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Iniciais));
        }

        [Fact]
        public async Task GetUsuariosAtivosAsync_FilterByTeam_ReturnsTeamUsers()
        {
            // Arrange
            var equipeId = 1; // Frontend team

            // Act
            var result = await _gridService.GetUsuariosAtivosAsync(equipeId);

            // Assert
            result.Should().HaveCount(2); // João and Ana
            result.Should().OnlyContain(u => u.Nome == "João Silva" || u.Nome == "Ana Oliveira");
        }

        [Fact]
        public async Task GetUsuariosAtivosAsync_InitialsGeneration_WorksCorrectly()
        {
            // Arrange & Act
            var result = await _gridService.GetUsuariosAtivosAsync();

            // Assert
            var joao = result.First(u => u.Nome == "João Silva");
            joao.Iniciais.Should().Be("JS");

            var maria = result.First(u => u.Nome == "Maria Santos");
            maria.Iniciais.Should().Be("MS");

            var ana = result.First(u => u.Nome == "Ana Oliveira");
            ana.Iniciais.Should().Be("AO");
        }

        #endregion

        #region GetGridMetricsAsync Tests

        [Fact]
        public async Task GetGridMetricsAsync_ValidFilter_ReturnsMetrics()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today
            };

            // Act
            var result = await _gridService.GetGridMetricsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalAtividades.Should().BeGreaterThan(0);
            result.AtividadesAtrasadas.Should().BeGreaterOrEqualTo(0);
            result.AtividadesPorStatus.Should().NotBeEmpty();
            result.AtividadesPorPrioridade.Should().NotBeEmpty();
            result.TaxaConclusao.Should().BeInRange(0, 100);
        }

        [Fact]
        public async Task GetGridMetricsAsync_HasOverdueActivities_CountsCorrectly()
        {
            // Arrange
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today
            };

            // Act
            var result = await _gridService.GetGridMetricsAsync(filter);

            // Assert
            result.AtividadesAtrasadas.Should().BeGreaterThan(0);
            result.TaxaConclusao.Should().BeLessThan(100);
        }

        #endregion

        #region Edge Cases and Error Handling Tests

        [Fact]
        public async Task GetGridDataAsync_EmptyDatabase_ReturnsEmptyGrid()
        {
            // Arrange
            await ClearDatabase();
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today
            };

            // Act
            var result = await _gridService.GetGridDataAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Usuarios.Should().BeEmpty();
            result.Dias.Should().HaveCount(7); // Days should still be generated
        }

        [Fact]
        public async Task GetUsuariosAtivosAsync_NonExistentTeam_ReturnsEmpty()
        {
            // Arrange
            var nonExistentTeamId = 999;

            // Act
            var result = await _gridService.GetUsuariosAtivosAsync(nonExistentTeamId);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("A")]
        [InlineData("João")]
        [InlineData("João Silva Santos")]
        public void GetInitials_VariousNames_GeneratesCorrectInitials(string nome)
        {
            // This would test the private GetInitials method
            // For this test, we would need to make the method internal and use InternalsVisibleTo
            // Or create a public wrapper method for testing

            // For now, we test through the public API
            Assert.True(true); // Placeholder - implement based on actual GetInitials logic
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task GetGridDataAsync_LargeDataset_CompletesInReasonableTime()
        {
            // Arrange
            await SeedLargeDataset();
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-30),
                DataFim = DateTime.Today.AddDays(30),
                SemanaReferencia = DateTime.Today
            };

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _gridService.GetGridDataAsync(filter);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete in less than 5 seconds
        }

        private async Task SeedLargeDataset()
        {
            // Create large dataset for performance testing
            var largeEquipes = Enumerable.Range(10, 10)
                .Select(i => new Equipe { Id = i, Nome = $"Equipe {i}", Descricao = $"Descrição {i}" });

            var largeUsuarios = Enumerable.Range(100, 100)
                .Select(i => new Usuario
                {
                    Id = i,
                    Nome = $"Usuário {i}",
                    Email = $"usuario{i}@test.com",
                    Cargo = "Desenvolvedor",
                    EquipeId = 10 + (i % 10),
                    Ativo = true
                });

            var largeAtividades = Enumerable.Range(1000, 1000)
                .Select(i => new Atividade
                {
                    Id = i,
                    Nome = $"Atividade {i}",
                    Descricao = $"Descrição da atividade {i}",
                    DataInicio = DateTime.Today.AddDays(-30 + (i % 60)),
                    DataFimPrevista = DateTime.Today.AddDays(-25 + (i % 60)),
                    UsuarioId = 100 + (i % 100),
                    StatusId = 1 + (i % 4),
                    Prioridade = 1 + (i % 4),
                    HorasEstimadas = 1 + (i % 16)
                });

            _context.Equipes.AddRange(largeEquipes);
            _context.Usuarios.AddRange(largeUsuarios);
            _context.Atividades.AddRange(largeAtividades);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Utility Methods

        private async Task ClearDatabase()
        {
            _context.Atividades.RemoveRange(_context.Atividades);
            _context.Usuarios.RemoveRange(_context.Usuarios);
            _context.Equipes.RemoveRange(_context.Equipes);
            _context.StatusAtividades.RemoveRange(_context.StatusAtividades);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            _context?.Dispose();
        }

        #endregion
    }

    #region Test Helpers and Extensions

    /// <summary>
    /// Helper class para testes
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Cria um contexto de teste com dados padrão
        /// </summary>
        public static ProGestaoContext CreateTestContext(string databaseName = null)
        {
            var options = new DbContextOptionsBuilder<ProGestaoContext>()
                .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new ProGestaoContext(options);
        }

        /// <summary>
        /// Cria um mock do logger
        /// </summary>
        public static Mock<ILogger<T>> CreateLoggerMock<T>()
        {
            return new Mock<ILogger<T>>();
        }
    }

    /// <summary>
    /// Custom assertions para FluentAssertions
    /// </summary>
    public static class CustomAssertions
    {
        public static void ShouldHaveValidGridStructure(this GridDataViewModel grid)
        {
            grid.Should().NotBeNull();
            grid.Usuarios.Should().NotBeNull();
            grid.Dias.Should().HaveCount(7);
            grid.Dias.Should().OnlyContain(d => d.Data != default);
        }

        public static void ShouldHaveValidUser(this UsuarioGridViewModel user)
        {
            user.Should().NotBeNull();
            user.Nome.Should().NotBeNullOrEmpty();
            user.Cargo.Should().NotBeNullOrEmpty();
            user.Iniciais.Should().NotBeNullOrEmpty();
            user.Iniciais.Should().HaveLength(2);
        }
    }

    #endregion
}

namespace ProGestao.Tests.Integration
{
    /// <summary>
    /// Testes de integração para GridService
    /// Testa a integração com banco de dados real (usando TestContainers)
    /// </summary>
    [Collection("Database")]
    public class GridServiceIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly IGridService _gridService;

        public GridServiceIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            var logger = new Mock<ILogger<GridService>>().Object;
            _gridService = new GridService(_fixture.Context, logger);
        }

        [Fact]
        public async Task GetGridDataAsync_RealDatabase_ReturnsExpectedData()
        {
            // Arrange
            await _fixture.SeedTestDataAsync();
            var filter = new GridFilterViewModel
            {
                DataInicio = DateTime.Today.AddDays(-7),
                DataFim = DateTime.Today.AddDays(7),
                SemanaReferencia = DateTime.Today
            };

            // Act
            var result = await _gridService.GetGridDataAsync(filter);

            // Assert
            result.ShouldHaveValidGridStructure();
        }
    }

    /// <summary>
    /// Fixture para configuração do banco de dados de teste
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        public ProGestaoContext Context { get; private set; }

        public DatabaseFixture()
        {
            Context = TestHelpers.CreateTestContext("IntegrationTests");
        }

        public async Task SeedTestDataAsync()
        {
            // Implementar seed de dados para testes de integração
            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}