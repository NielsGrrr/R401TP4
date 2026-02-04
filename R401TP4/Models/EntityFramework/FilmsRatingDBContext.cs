using Microsoft.EntityFrameworkCore;

namespace R401TP4.Models.EntityFramework
{
    public partial class FilmsRatingDBContext : DbContext
    {
        public FilmsRatingDBContext() { }
        public FilmsRatingDBContext(DbContextOptions<FilmsRatingDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Film> Films { get; set; } = null!;

        public virtual DbSet<Notation> Notations { get; set; } = null!;

        public virtual DbSet<Utilisateur> Utilisateurs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.HasKey(e => e.UtilisateurId).HasName("pk_utilisateur");

                entity.HasIndex(e => e.Mail).IsUnique();

                entity.Property(e => e.Datecreation).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Film>(entity =>
            {
                entity.HasKey(e => e.FilmId).HasName("pk_film");

                entity.HasIndex(e => e.Titre);
            });

            modelBuilder.Entity<Notation>(entity =>
            {
                entity.HasKey(e => new { e.UtilisateurId, e.FilmId }).HasName("pk_notation");

                entity.HasOne(e => e.UtilisateurNotant)
                      .WithMany(u => u.NotesUtilisateur)
                      .HasForeignKey(e => e.UtilisateurId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_notation_utilisateur");

                entity.HasOne(e => e.FilmNote)
                      .WithMany(f => f.NotesFilm)
                      .HasForeignKey(e => e.FilmId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_notation_film");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseLoggerFactory(MyLoggerFactory)
                            .EnableSensitiveDataLogging()
                            .UseNpgsql("Server=localhost;port=5432;Database=TheMovieDB; uid=postgres; password=postgres;");
        }
    }
}
