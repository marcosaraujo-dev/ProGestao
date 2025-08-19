namespace ProGestao.Extensions
{
    public static class AtividadeExtensions
    {
        public static IQueryable<Models.Atividade> ComFiltros(
            this IQueryable<Models.Atividade> query,
            int? projetoId = null,
            int? usuarioId = null,
            int? statusId = null,
            int? tipoAtividadeId = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            int? prioridade = null,
            bool? somenteAtrasadas = null)
        {
            if (projetoId.HasValue)
                query = query.Where(a => a.ProjetoId == projetoId.Value);

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (statusId.HasValue)
                query = query.Where(a => a.StatusId == statusId.Value);

            if (tipoAtividadeId.HasValue)
                query = query.Where(a => a.TipoAtividadeId == tipoAtividadeId.Value);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataInicio >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataInicio <= dataFim.Value);

            if (prioridade.HasValue)
                query = query.Where(a => a.Prioridade == prioridade.Value);

            if (somenteAtrasadas == true)
            {
                var hoje = DateTime.Today;
                query = query.Where(a => a.DataFimPrevista < hoje &&
                                        a.DataFimReal == null &&
                                        a.Status.Nome != "Concluída");
            }

            return query;
        }

        public static IQueryable<Models.Atividade> OrdenadoPor(
            this IQueryable<Models.Atividade> query,
            string ordenacao = "DataCriacao")
        {
            return ordenacao.ToLower() switch
            {
                "nome" => query.OrderBy(a => a.Nome),
                "projeto" => query.OrderBy(a => a.Projeto.Nome),
                "usuario" => query.OrderBy(a => a.Usuario.Nome),
                "status" => query.OrderBy(a => a.Status.Ordem),
                "prioridade" => query.OrderByDescending(a => a.Prioridade),
                "datainicio" => query.OrderBy(a => a.DataInicio),
                "datafimprevista" => query.OrderBy(a => a.DataFimPrevista),
                "datacriacao" => query.OrderByDescending(a => a.DataCriacao),
                _ => query.OrderByDescending(a => a.DataCriacao)
            };
        }
    }
}
