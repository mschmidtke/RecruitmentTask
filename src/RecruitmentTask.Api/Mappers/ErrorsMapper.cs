using RecruitmentTask.Contracts.Validations;
using RecruitmentTask.Core;

namespace RecruitmentTask.Api.Mappers
{
    public static class ErrorsMapper
    {
        public static ValidationResult ToContracts(this OperationResult operationResult)
        {
            return new ValidationResult
            {
                Errors = operationResult.Errors.Select(error => new ValidationError
                    { ErrorCode = error.ErrorCode, PropertyName = error.PropertyName })
            };
        }

        public static ValidationResult ToContracts<T>(this OperationResultWithValue<T> operationResult)
        {
            return new ValidationResult
            {
                Errors = operationResult.Errors.Select(error => new ValidationError
                    { ErrorCode = error.ErrorCode, PropertyName = error.PropertyName })
            };
        }
    }
}
