namespace Epson.Model.Request
{
    public class CompetitorInformationModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public decimal DistyPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public decimal EndUserPrice { get; set; }
    }
}
