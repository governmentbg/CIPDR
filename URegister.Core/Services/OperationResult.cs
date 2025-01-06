namespace URegister.Core.Services
{
    /// <summary>
    /// Резултат от операция в базата данни
    /// </summary>
    public class OperationResult
    {
        public OperationResult(bool isSuccess = true) : this(isSuccess, String.Empty)
        {
        }

        public OperationResult(string errorMessage) : this(false, errorMessage)
        {
        }

        public OperationResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Успешна ли е операцията
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Съобщение за грешка
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Допълнителна информация
        /// </summary>
        public object CustomObject { get; set; }
    }

    /// <summary>
    /// Резултат от операция по запис
    /// </summary>
    public class SaveOperationResult : OperationResult
    {
        public SaveOperationResult(bool success, object addedObjectId) : base(success, String.Empty)
        {
            AddedObjectId = addedObjectId;
        }

        public SaveOperationResult(string errorMessage) : base(false, errorMessage) { }      

        public object AddedObjectId { get; set; } = 0;
    }
}
