namespace ProGestao.Common
{
    /// <summary>
    /// Classe para resultado de validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }
    }
}
