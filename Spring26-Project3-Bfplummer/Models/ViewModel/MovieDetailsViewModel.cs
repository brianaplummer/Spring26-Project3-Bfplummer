namespace Spring26_Project3_Bfplummer.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie? Movie { get; set; }
        public List<Actor> Actors { get; set; } = new();
        public string[] MovieReviews { get; set; } = [];
        public double[] ReviewSentiments { get; set; } = [];
        public double OverAllSentiment { get; set; }
    }
}