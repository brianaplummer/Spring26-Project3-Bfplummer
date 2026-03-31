using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring26_Project3_Bfplummer.Data;
using Spring26_Project3_Bfplummer.Models;

namespace Spring26_Project3_Bfplummer.Controllers
{
    public class ActorMovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var relationships = _context.ActorsMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie);

            return View(await relationships.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActorId,MovieId")] ActorMovie actorMovie)
        {
            if (_context.ActorsMovies.Any(am => am.ActorId == actorMovie.ActorId && am.MovieId == actorMovie.MovieId))
            {
                ModelState.AddModelError("", "That actor/movie relationship already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorsMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie)
                .FirstOrDefaultAsync(am => am.Id == id);

            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorsMovies.FindAsync(id);
            if (actorMovie == null) return NotFound();

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActorId,MovieId")] ActorMovie actorMovie)
        {
            if (id != actorMovie.Id) return NotFound();

            bool duplicateExists = _context.ActorsMovies.Any(am =>
                am.ActorId == actorMovie.ActorId &&
                am.MovieId == actorMovie.MovieId &&
                am.Id != actorMovie.Id);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "That actor/movie relationship already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorsMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie)
                .FirstOrDefaultAsync(am => am.Id == id);

            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorsMovies.FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorsMovies.Remove(actorMovie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}