using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spring26_Project3_Bfplummer.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Display(Name = "IMDB Link")]
        public string? IMDB_Link { get; set; }

        public string? Genre { get; set; }

        [Display(Name = "Release Year")]
        public int Release_Year { get; set; }

        public byte[]? Movie_Poster { get; set; }

        [NotMapped]
        [Display(Name = "Poster Upload")]
        public IFormFile? PhotoFile { get; set; }

        public ICollection<ActorMovie>? ActorMovies { get; set; }
    }
}