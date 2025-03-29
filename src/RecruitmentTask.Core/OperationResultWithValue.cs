namespace RecruitmentTask.Core;

public class OperationResultWithValue<T>
{
    public bool IsSuccess => !Errors.Any();

    public T Value;

    public IEnumerable<OperationError> Errors { get; }

    public static OperationResultWithValue<T> Success(T value)
    {
        return new OperationResultWithValue<T>(value, Enumerable.Empty<OperationError>());
    }

    public static OperationResultWithValue<T> Fail(OperationError error)
    {
        return new OperationResultWithValue<T>(new List<OperationError> { error });
    }

    public static OperationResultWithValue<T> Fail(IEnumerable<OperationError> errors)
    {
        return new OperationResultWithValue<T>(errors);
    }

    private OperationResultWithValue(T value, IEnumerable<OperationError> errors)
    {
        Value = value;
        Errors = errors;
    }
    private OperationResultWithValue(IEnumerable<OperationError> errors)
    {
        Errors = errors;
    }
}