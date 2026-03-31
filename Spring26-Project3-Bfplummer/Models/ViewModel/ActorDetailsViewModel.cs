namespace Spring26_Project3_Bfplummer.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public Actor? Actor { get; set; }
        public List<Movie> Movies { get; set; } = new();
        public string[] Tweets { get; set; } = [];
        public double[] TweetSentiments { get; set; } = [];
        public double OverAllActorSentiment { get; set; }
    }
}