using System.Xml.Serialization;

namespace RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Models
{
    [XmlRoot(ElementName = "tabela_kursow")]
    public class ExchangeRates
    {
        [XmlElement(ElementName = "data_notowania")]
        public DateTime QuotationDate { get; set; }

        [XmlElement(ElementName = "data_publikacji")]
        public DateTime PublicationDate { get; set; }

        [XmlElement(ElementName = "pozycja")]
        public List<Currency> Currencies { get; set; }
    }

    [XmlRoot(ElementName = "pozycja")]
    public class Currency
    {

        [XmlElement(ElementName = "nazwa_waluty")]
        public string Name { get; set; }

        [XmlElement(ElementName = "przelicznik")]
        public int Rate { get; set; }

        [XmlElement(ElementName = "kod_waluty")]
        public string Code { get; set; }

        [XmlElement(ElementName = "kurs_kupna" )]
        
        public string BuyCourse { get; set; }

        [XmlElement(ElementName = "kurs_sprzedazy")]
        public string SellCourse { get; set; }
    }
}
