using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Client.ReplayDb;

public class CheckersReplayDb : DbContext
{
    public DbSet<ReplayGame> ReplayGames { get; set; }
    public DbSet<ReplayMove> ReplayMoves { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            "Server=(LocalDB)\\MSSQLLocalDB;Database=CheckersReplayDb;" +
            "Integrated Security=True;MultipleActiveResultSets=True");
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ReplayMove>(e =>
        {
            e.HasOne(m => m.Game)
             .WithMany(g => g.Moves)
             .HasForeignKey(m => m.ReplayGameId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
