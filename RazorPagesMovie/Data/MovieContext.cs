using Microsoft.EntityFrameworkCore;

using RazorPagesMovie.Models;

namespace RazorPagesMovie.Data
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; } = default!;
    }
}