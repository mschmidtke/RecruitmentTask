namespace RecruitmentTask.Core;

public class OperationError
{
    public string ErrorCode { get; }
    public string PropertyName { get; }

    public static OperationError Create(string errorCode, string propertyName)
    {
        return new OperationError(errorCode, propertyName);
    }

    private OperationError(string errorCode, string propertyName)
    {
        ErrorCode = errorCode;
        PropertyName = propertyName;
    }
}