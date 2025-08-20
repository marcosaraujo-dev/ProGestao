namespace ProGestao.ViewModels
{
    /// <summary>
    /// ViewModel para operações de resultado
    /// </summary>
    public class OperationResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public object? Data { get; set; }

        public static OperationResultViewModel CreateSuccess(string message = "", object? data = null)
        {
            return new OperationResultViewModel
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static OperationResultViewModel CreateError(string message, List<string>? errors = null)
        {
            return new OperationResultViewModel
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
