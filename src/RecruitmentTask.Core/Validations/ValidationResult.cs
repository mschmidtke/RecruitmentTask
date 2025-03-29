namespace RecruitmentTask.Core.Validations
{
    public class ValidationResult
    {
        public bool IsSuccess => !Errors.Any();

        public IEnumerable<ValidationError> Errors { get; }

        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Errors = errors;
        }
    }
}
