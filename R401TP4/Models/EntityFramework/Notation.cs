using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R401TP4.Models.EntityFramework
{
    [Table("t_j_notation_not")]
    public partial class Notation
    {
        [Key]
        [Column("utl_id")]
        public int UtilisateurId { get; set; }

        [Key]
        [Column("flm_id")]
        public int FilmId { get; set; }

        [Column("not_note")]
        [Range(0, 5)]
        public int Note { get; set; }

        [InverseProperty("NotesUtilisateur")]
        public virtual Utilisateur UtilisateurNotant { get; set; } = null!;

        [InverseProperty("NotesFilm")]
        public virtual Film FilmNote { get; set; } = null!;
    }
}
