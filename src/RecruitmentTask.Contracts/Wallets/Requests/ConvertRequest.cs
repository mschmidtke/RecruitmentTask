namespace RecruitmentTask.Contracts.Wallets.Requests;

public class ConvertRequest
{
    public string FromCurrencyCode {get; set; }
    public string ToCurrencyCode { get; set; }
    public decimal Amount { get; set; }
}