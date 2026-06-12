using CheckersGame.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Data.Context;

// DbContext = the main object we use to talk to the database.
// Every time a Razor Page or Controller needs the DB, ASP.NET injects this class.
public class CheckersCentralDb : DbContext
{
    // Constructor — ASP.NET passes the connection string options automatically
    public CheckersCentralDb(DbContextOptions<CheckersCentralDb> options) : base(options) { }

    // Each DbSet = one table in the database
    public DbSet<Country>    Countries   { get; set; }
    public DbSet<Player>     Players     { get; set; }
    public DbSet<Game>       Games       { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }

    // OnModelCreating = where we configure table rules, constraints, and relationships
    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Countries ────────────────────────────────────────────────────
        // No two countries can have the same name
        mb.Entity<Country>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        // ── Players ───────────────────────────────────────────────────────
        mb.Entity<Player>(e =>
        {
            // The player CHOOSES their own ID — it is NOT auto-generated
            e.Property(p => p.Id).ValueGeneratedNever();

            // DB-level CHECK: ID must be 1–1000
            e.ToTable(t => t.HasCheckConstraint("CK_Player_Id", "[Id] BETWEEN 1 AND 1000"));

            // DB-level CHECK: phone must be exactly 10 digits
            e.ToTable(t => t.HasCheckConstraint("CK_Player_Phone",
                "LEN([Phone]) = 10 AND [Phone] NOT LIKE '%[^0-9]%'"));

            // Each player belongs to one country
            e.HasOne(p => p.Country)
             .WithMany(c => c.Players)
             .HasForeignKey(p => p.CountryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Games ─────────────────────────────────────────────────────────
        // Store Status as an integer (0=InProgress, 1=HumanWon, 2=ServerWon, 3=Abandoned)
        mb.Entity<Game>(e =>
        {
            e.Property(g => g.Status).HasConversion<int>();
        });

        // ── GamePlayers (join table: links Players to Games) ──────────────
        mb.Entity<GamePlayer>(e =>
        {
            // A player can only appear once per game
            e.HasIndex(gp => new { gp.GameId, gp.PlayerId }).IsUnique();

            // If a Game is deleted → delete all its GamePlayers rows automatically
            e.HasOne(gp => gp.Game)
             .WithMany(g => g.GamePlayers)
             .HasForeignKey(gp => gp.GameId)
             .OnDelete(DeleteBehavior.Cascade);

            // If a Player is deleted → delete their GamePlayers rows automatically
            e.HasOne(gp => gp.Player)
             .WithMany(p => p.GamePlayers)
             .HasForeignKey(gp => gp.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed data — pre-fill the Countries table ──────────────────────
        mb.Entity<Country>().HasData(
            new Country { Id = 1, Name = "Israel" },
            new Country { Id = 2, Name = "USA" },
            new Country { Id = 3, Name = "UK" },
            new Country { Id = 4, Name = "France" },
            new Country { Id = 5, Name = "Germany" },
            new Country { Id = 6, Name = "Spain" },
            new Country { Id = 7, Name = "Italy" },
            new Country { Id = 8, Name = "Russia" }
        );
    }
}
