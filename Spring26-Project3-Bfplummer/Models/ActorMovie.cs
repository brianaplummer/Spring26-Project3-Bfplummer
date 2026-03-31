namespace Spring26_Project3_Bfplummer.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }

        public int ActorId { get; set; }
        public Actor? Actor { get; set; }

        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
    }
}


