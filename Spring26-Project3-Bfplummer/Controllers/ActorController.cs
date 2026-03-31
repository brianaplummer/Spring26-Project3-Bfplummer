using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Spring26_Project3_Bfplummer.Data;
using Spring26_Project3_Bfplummer.Models;
using Spring26_Project3_Bfplummer.Models.ViewModels;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VaderSharp2;

namespace Spring26_Project3_Bfplummer.Controllers
{
    public class ActorController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private const string AiDeployment = "gpt-4.1-nano";

        private record class Tweet(string Username, string Text);
        private record class Tweets(Tweet[] Items);

        public ActorController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> Photo(int? id)
        {
            if (id == null) return BadRequest();

            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null || actor.Photo == null) return NotFound();

            return File(actor.Photo, "image/jpg");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Age,Gender,IMDB_Link,PhotoFile")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                if (actor.PhotoFile != null)
                {
                    using var memoryStream = new MemoryStream();
                    await actor.PhotoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null) return NotFound();

            var movies = await _context.ActorsMovies
                .Include(am => am.Movie)
                .Where(am => am.ActorId == id)
                .Select(am => am.Movie!)
                .ToListAsync();

            string[] tweetsForView = [];
            double[] sentimentsForView = [];
            double sentimentAverage = 0;

            try
            {
                var apiEndpoint = new Uri(_config["AI_Credentials:Endpoint"]!);
                var apiCredential = new ApiKeyCredential(_config["AI_Credentials:API_Key"]!);
                ChatClient client = new AzureOpenAIClient(apiEndpoint, apiCredential).GetChatClient(AiDeployment);
                
                var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };

                JsonNode schema = options.GetJsonSchemaAsNode(typeof(Tweets), new()
                {
                    TreatNullObliviousAsNonNullable = true,
                });

                var chatCompletionOptions = new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        "XTwitterApiJson",
                        BinaryData.FromString(schema.ToString()),
                        jsonSchemaIsStrict: true)
                };

                var messages = new ChatMessage[]
                {
                    new SystemChatMessage("You represent the X/Twitter API and return JSON data."),
                    new UserChatMessage($"Generate 10 tweets from different users about the actor {actor.Name}.")
                };

                ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages, chatCompletionOptions);

                string jsonString = result.Value.Content.FirstOrDefault()?.Text ?? @"{""Items"":[]}";
                Tweets tweets = JsonSerializer.Deserialize<Tweets>(jsonString) ?? new([]);

                var analyzer = new SentimentIntensityAnalyzer();
                tweetsForView = new string[tweets.Items.Length];
                sentimentsForView = new double[tweets.Items.Length];

                double total = 0;

                for (int i = 0; i < tweets.Items.Length; i++)
                {
                    var tweet = tweets.Items[i];
                    var sentiment = analyzer.PolarityScores(tweet.Text);
                    tweetsForView[i] = $"{tweet.Username}: \"{tweet.Text}\"";
                    sentimentsForView[i] = sentiment.Compound;
                    total += sentiment.Compound;
                }

                if (tweets.Items.Length > 0)
                {
                    sentimentAverage = total / tweets.Items.Length;
                }
            }
            catch
            {
                tweetsForView = ["AI tweets could not be loaded right now."];
                sentimentsForView = [0];
                sentimentAverage = 0;
            }

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = movies,
                Tweets = tweetsForView,
                TweetSentiments = sentimentsForView,
                OverAllActorSentiment = sentimentAverage
            };

            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Age,Gender,IMDB_Link,PhotoFile")] Actor actor)
        {
            if (id != actor.Id) return NotFound();

            var existingActor = await _context.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingActor == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (actor.PhotoFile != null)
                    {
                        using var memoryStream = new MemoryStream();
                        await actor.PhotoFile.CopyToAsync(memoryStream);
                        actor.Photo = memoryStream.ToArray();
                    }
                    else
                    {
                        actor.Photo = existingActor.Photo;
                    }

                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Actors.Any(a => a.Id == actor.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var relationships = _context.ActorsMovies.Where(am => am.ActorId == id);
            _context.ActorsMovies.RemoveRange(relationships);

            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}