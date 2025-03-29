namespace RecruitmentTask.Contracts.Validations
{
    public class ValidationError
    {
        public string ErrorCode { get; set; }
        public string PropertyName { get; set; }
    }

    public class ValidationResult
    {
        public IEnumerable<ValidationError> Errors { get; set; }
    }
}
