namespace RecruitmentTask.Core.ExchangeRate.Model;

public class Currency
{
    public Currency(string name, int rate, string code, decimal buyCourse, decimal sellCourse)
    {
        Name = name;
        Rate = rate;
        Code = code;
        BuyCourse = buyCourse;
        SellCourse = sellCourse;
    }

    public string Name { get; }

    public int Rate { get; }

    public string Code { get; }

    public decimal BuyCourse { get; }

    public decimal SellCourse { get; }
}