
namespace FleetManager.Common.Results
{
    //KLASA GENERYCZNA(Dla operacji z danymi)
    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base()
        {
            Value = value;
        }

        private Result(string error, ResultErrorType errorType) : base(error, errorType)
        {}

        private Result(T value, string error, ResultErrorType errorType) : base(error, errorType)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(value);
        public static new Result<T> NotFound(string error) => new(error, ResultErrorType.NotFound);
        public static new Result<T> Validation(string error) => new(error, ResultErrorType.Validation);
        public static Result<T> Conflict(T value, string error) => new(value, error, ResultErrorType.Conflict);
    }
}
