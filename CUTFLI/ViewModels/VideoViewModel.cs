namespace CUTFLI.ViewModels
{
    public class VideoViewModel
    {
        public int? Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string VideoName { get; set; }
        public byte[] Video { get; set; }
    }
}
