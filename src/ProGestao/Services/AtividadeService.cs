using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels;

namespace ProGestao.Services
{
    
    public class AtividadeService : IAtividadeService
    {
        private readonly ProGestaoContext _context;

        public AtividadeService(ProGestaoContext context)
        {
            _context = context;
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesByFiltroAsync(int? projetoId = null, int? usuarioId = null, int? statusId = null)
        {
            var query = _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .AsQueryable();

            if (projetoId.HasValue)
                query = query.Where(a => a.ProjetoId == projetoId.Value);

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (statusId.HasValue)
                query = query.Where(a => a.StatusId == statusId.Value);

            var atividades = await query
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();

            return atividades.Select(MapToViewModel).ToList();
        }

        public async Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            return atividade != null ? MapToViewModel(atividade) : null;
        }

        public async Task<bool> CreateAtividadeAsync(AtividadeViewModel atividadeVm)
        {
            var atividade = new Atividade
            {
                Nome = atividadeVm.Nome,
                Descricao = atividadeVm.Descricao,
                ProjetoId = atividadeVm.ProjetoId,
                TipoAtividadeId = atividadeVm.TipoAtividadeId,
                StatusId = atividadeVm.StatusId,
                UsuarioId = atividadeVm.UsuarioId,
                DataInicio = atividadeVm.DataInicio,
                DataFimPrevista = atividadeVm.DataFimPrevista,
                DataFimReal = atividadeVm.DataFimReal,
                HorasEstimadas = atividadeVm.HorasEstimadas,
                HorasReais = atividadeVm.HorasReais,
                Prioridade = atividadeVm.Prioridade,
                Observacoes = atividadeVm.Observacoes,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.Atividades.Add(atividade);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAtividadeAsync(AtividadeViewModel atividadeVm)
        {
            var atividade = await _context.Atividades.FindAsync(atividadeVm.Id);
            if (atividade == null) return false;

            atividade.Nome = atividadeVm.Nome;
            atividade.Descricao = atividadeVm.Descricao;
            atividade.ProjetoId = atividadeVm.ProjetoId;
            atividade.TipoAtividadeId = atividadeVm.TipoAtividadeId;
            atividade.StatusId = atividadeVm.StatusId;
            atividade.UsuarioId = atividadeVm.UsuarioId;
            atividade.DataInicio = atividadeVm.DataInicio;
            atividade.DataFimPrevista = atividadeVm.DataFimPrevista;
            atividade.DataFimReal = atividadeVm.DataFimReal;
            atividade.HorasEstimadas = atividadeVm.HorasEstimadas;
            atividade.HorasReais = atividadeVm.HorasReais;
            atividade.Prioridade = atividadeVm.Prioridade;
            atividade.Observacoes = atividadeVm.Observacoes;
            atividade.DataAtualizacao = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAtividadeAsync(int id)
        {
            var atividade = await _context.Atividades.FindAsync(id);
            if (atividade == null) return false;

            _context.Atividades.Remove(atividade);
            return await _context.SaveChangesAsync() > 0;
        }

        private static AtividadeViewModel MapToViewModel(Atividade atividade)
        {
            return new AtividadeViewModel
            {
                Id = atividade.Id,
                Nome = atividade.Nome,
                Descricao = atividade.Descricao,
                ProjetoId = atividade.ProjetoId,
                ProjetoNome = atividade.Projeto.Nome,
                TipoAtividadeId = atividade.TipoAtividadeId,
                TipoAtividadeNome = atividade.TipoAtividade.Nome,
                StatusId = atividade.StatusId,
                StatusNome = atividade.Status.Nome,
                StatusCor = atividade.Status.Cor,
                UsuarioId = atividade.UsuarioId,
                UsuarioNome = atividade.Usuario.Nome,
                DataInicio = atividade.DataInicio,
                DataFimPrevista = atividade.DataFimPrevista,
                DataFimReal = atividade.DataFimReal,
                HorasEstimadas = atividade.HorasEstimadas,
                HorasReais = atividade.HorasReais,
                Prioridade = atividade.Prioridade,
                Observacoes = atividade.Observacoes
            };
        }
    }
}