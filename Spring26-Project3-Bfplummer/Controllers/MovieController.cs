using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Spring26_Project3_Bfplummer.Data;
using Spring26_Project3_Bfplummer.Models;
using Spring26_Project3_Bfplummer.Models.ViewModels;
using System.ClientModel;
using VaderSharp2;

namespace Spring26_Project3_Bfplummer.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private const string AiDeployment = "gpt-4.1-nano";

        public MovieController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public async Task<IActionResult> Photo(int? id)
        {
            if (id == null) return BadRequest();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null || movie.Movie_Poster == null) return NotFound();

            return File(movie.Movie_Poster, "image/jpg");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDB_Link,Genre,Release_Year,PhotoFile")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                if (movie.PhotoFile != null)
                {
                    using var memoryStream = new MemoryStream();
                    await movie.PhotoFile.CopyToAsync(memoryStream);
                    movie.Movie_Poster = memoryStream.ToArray();
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            var actors = await _context.ActorsMovies
                .Include(am => am.Actor)
                .Where(am => am.MovieId == id)
                .Select(am => am.Actor!)
                .ToListAsync();

            string[] reviews = [];
            double[] reviewSentiments = [];
            double sentimentAverage = 0;

            try
            {
                var apiEndpoint = new Uri(_config["AI_Credentials:Endpoint"]!);
                var apiCredential = new ApiKeyCredential(_config["AI_Credentials:API_Key"]!);
                ChatClient client = new AzureOpenAIClient(apiEndpoint, apiCredential).GetChatClient(AiDeployment);

                string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy" };
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage($"You represent a group of {personas.Length} film critics who have the following personalities: {string.Join(",", personas)}. When you receive a question, respond as each member of the group with each response separated by a '|', but do not label them."),
                    new UserChatMessage($"How would you rate the movie {movie.Title} released in {movie.Release_Year} out of 10 in 150 words or less?")
                };

                ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
                reviews = result.Value.Content[0].Text.Split('|').Select(r => r.Trim()).ToArray();

                var analyzer = new SentimentIntensityAnalyzer();
                reviewSentiments = new double[reviews.Length];
                double total = 0;

                for (int i = 0; i < reviews.Length; i++)
                {
                    var sentiment = analyzer.PolarityScores(reviews[i]);
                    reviewSentiments[i] = sentiment.Compound;
                    total += sentiment.Compound;
                }

                if (reviews.Length > 0)
                {
                    sentimentAverage = total / reviews.Length;
                }
            }
            catch
            {
                reviews = ["AI reviews could not be loaded right now."];
                reviewSentiments = [0];
                sentimentAverage = 0;
            }

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = actors,
                MovieReviews = reviews,
                ReviewSentiments = reviewSentiments,
                OverAllSentiment = sentimentAverage
            };

            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDB_Link,Genre,Release_Year,PhotoFile")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            var existingMovie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existingMovie == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (movie.PhotoFile != null)
                    {
                        using var memoryStream = new MemoryStream();
                        await movie.PhotoFile.CopyToAsync(memoryStream);
                        movie.Movie_Poster = memoryStream.ToArray();
                    }
                    else
                    {
                        movie.Movie_Poster = existingMovie.Movie_Poster;
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(m => m.Id == movie.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var relationships = _context.ActorsMovies.Where(am => am.MovieId == id);
            _context.ActorsMovies.RemoveRange(relationships);

            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}