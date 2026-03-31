using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spring26_Project3_Bfplummer.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public int Age { get; set; }

        public string? Gender { get; set; }

        [Display(Name = "IMDB Link")]
        public string? IMDB_Link { get; set; }

        public byte[]? Photo { get; set; }

        [NotMapped]
        [Display(Name = "Photo Upload")]
        public IFormFile? PhotoFile { get; set; }

        public ICollection<ActorMovie>? ActorMovies { get; set; }
    }
}







