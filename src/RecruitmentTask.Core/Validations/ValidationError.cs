namespace RecruitmentTask.Core.Validations;

public class ValidationError
{
    public string ErrorCode { get; }
    public string PropertyName { get; }

    public static ValidationError Create(string errorCode, string propertyName)
    {
        return new ValidationError(errorCode, propertyName);
    }

    private ValidationError(string errorCode, string propertyName)
    {
        ErrorCode = errorCode;
        PropertyName = propertyName;
    }
}