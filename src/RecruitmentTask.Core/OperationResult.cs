namespace RecruitmentTask.Core
{
    public class OperationResult
    {
        public bool IsSuccess => !Errors.Any();

        public IEnumerable<OperationError> Errors { get; }

        public static OperationResult Success()
        {
            return new OperationResult([]);
        }

        public static OperationResult Fail(OperationError error)
        {
            return new OperationResult(new List<OperationError> { error });
        }

        public static OperationResult Fail(IEnumerable<OperationError> errors)
        {
            return new OperationResult(errors);
        }

        public OperationResult(IEnumerable<OperationError> errors)
        {
            Errors = errors;
        }
    }
}
