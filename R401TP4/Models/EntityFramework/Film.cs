using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R401TP4.Models.EntityFramework
{

    [Table("t_e_film_flm")]
    public partial class Film
    {
        [Key]
        [Column("flm_id")]
        public int FilmId { get; set; }

        [Column("flm_titre")]
        [StringLength(100)]
        public string Titre { get; set; } = null!;

        [Column("flm_resume")]
        public string? Resume { get; set; }

        [Column("flm_datesortie", TypeName = "date")]
        public DateTime? Datesortie { get; set; }

        [Column("flm_duree")]
        [Precision(3,0)]
        public decimal? Duree { get; set; }

        [Column("flm_genre")]
        [StringLength(30)]
        public string? Genre { get; set; }

        [InverseProperty("FilmNote")]
        public virtual ICollection<Notation> NotesFilm { get; set; } = new List<Notation>();
    }
}
