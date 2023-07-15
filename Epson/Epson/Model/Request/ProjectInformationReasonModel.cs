namespace Epson.Model.Request
{
    public class ProjectInformationReasonModel
    {
        public int Id { get; set; }
        public int ProjectInformationId { get; set; }
        public string SelectedReason { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
