using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly MovieContext _context;

        public EditModel(MovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Movie Movie { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Movie? movie = await _context.Movie
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (id == null)
            {
                return NotFound();
            }

            if (movie == null)
            {
                return NotFound();
            }
            Movie = movie;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Movie).State = EntityState.Modified;

            var movieToUpdate = await _context.Movie.FirstOrDefaultAsync(m => m.Id == Movie.Id);
            if (movieToUpdate == null)
            {
                return HandleDeletedMovie();
            }

            _context.Entry(movieToUpdate).Property(d => d.ConcurrencyToken).OriginalValue = Movie.ConcurrencyToken;
            if (await TryUpdateModelAsync<Movie>(
                movieToUpdate,
                "Movie",
                s => s.Title, s => s.ReleaseDate, s => s.Genre, s => s.Price, s => s.Rating))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Movie)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save. The movie was deleted by another user.");
                        return Page();
                    }

                    var databaseValues = (Movie)databaseEntry.ToObject();
                    await SetDbErrorMessage(databaseValues, clientValues, _context);

                    Movie.ConcurrencyToken = (byte[])databaseValues.ConcurrencyToken!;
                    ModelState.Remove($"{nameof(Movie)}.{nameof(Movie.ConcurrencyToken)}");
                }
            }

            return Page();
        }

        private async Task SetDbErrorMessage(Movie databaseValues, Movie clientValues, MovieContext context)
        {
            if (databaseValues.Title != clientValues.Title)
            {
                ModelState.AddModelError("Movie.Title", $"Current value: {databaseValues.Title}");
            }
            if (databaseValues.ReleaseDate != clientValues.ReleaseDate)
            {
                ModelState.AddModelError("Movie.ReleaseDate", $"Current value: {databaseValues.ReleaseDate:d}");
            }
            if (databaseValues.Genre != clientValues.Genre)
            {
                ModelState.AddModelError("Movie.Genre", $"Current value: {databaseValues.Genre}");
            }
            if (databaseValues.Price != clientValues.Price)
            {
                ModelState.AddModelError("Movie.Price", $"Current value: {databaseValues.Price:c}");
            }
            if (databaseValues.Rating != clientValues.Rating)
            {
                ModelState.AddModelError("Movie.Rating", $"Current value: {databaseValues.Rating}");
            }

            ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                + "was modified by another user after you got the original value. The "
                + "edit operation was canceled and the current values in the database "
                + "have been displayed. If you still want to edit this record, click "
                + "the Save button again. Otherwise click the Back to List hyperlink.");
            //return Task.CompletedTask;
        }

        private IActionResult HandleDeletedMovie()
        {
            ModelState.AddModelError(string.Empty, "Unable to save. The movie was deleted by another user.");
            return Page();
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}