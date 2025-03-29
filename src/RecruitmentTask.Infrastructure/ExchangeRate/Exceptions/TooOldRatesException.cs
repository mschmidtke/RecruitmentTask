namespace RecruitmentTask.Infrastructure.ExchangeRate.Exceptions
{
    public class TooOldRatesException : Exception
    {
        public TooOldRatesException() : base()
        { }

        public TooOldRatesException(short numberOfDays) : base($"Couldn't found rates in {numberOfDays} days.")
        { }

        public TooOldRatesException(string message) : base(message)
        { }

        public TooOldRatesException(string message, Exception innerException) : base(message, innerException)
        { }


    }
}
