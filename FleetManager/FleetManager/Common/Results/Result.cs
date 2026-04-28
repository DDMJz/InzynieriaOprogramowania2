namespace FleetManager.Common.Results
{ 
    // KLASA BAZOWA (Dla operacji Void)
    public class Result
    {
        public string? Error { get; }
        public ResultErrorType ErrorType { get; }
        public bool IsSuccess { get; }

        protected Result()
        {
            IsSuccess = true;
        }

        protected Result(string error, ResultErrorType errorType)
        {
            Error = error;
            ErrorType = errorType;
            IsSuccess = false;
        }

        public static Result Success() => new();
        public static Result NotFound(string error) => new(error, ResultErrorType.NotFound);
        public static Result Validation(string error) => new(error, ResultErrorType.Validation);
        public static Result Conflict(string error) => new(error, ResultErrorType.Conflict);
    }
}
