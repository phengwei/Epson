namespace Epson.Model.Request
{
    public class CompetitorInformationModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public Decimal Price { get; set; }
    }
}
