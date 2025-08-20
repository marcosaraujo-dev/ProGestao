namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel resumida para atividades
    /// </summary>
    public class AtividadeResumoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string StatusNome { get; set; } = string.Empty;
        public string StatusCor { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFimPrevista { get; set; }
        public int Prioridade { get; set; }
        public bool EstaAtrasada { get; set; }

        public string PrioridadeTexto => Prioridade switch
        {
            1 => "Baixa",
            2 => "Normal",
            3 => "Alta",
            4 => "Crítica",
            _ => "Normal"
        };
    }
}
    