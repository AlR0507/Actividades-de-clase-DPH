namespace NumericSeries.Models
{
    public class SeriesViewModel
    {
        public string Series { get; set; } = string.Empty;
        public int N { get; set; }
        public List<int> Result { get; set; }
        public string Message { get; set; } 
    }
}
