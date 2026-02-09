using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using R401TP4.Controllers;
using R401TP4.Models.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace R401TP4.Controllers.Tests
{
    [TestClass()]
    public class UtilisateursControllerTests
    {
        private UtilisateursController controller;
        private FilmsRatingDBContext context;

        [TestInitialize()]
        public void TestInitialize()
        {
            var builder = new DbContextOptionsBuilder<FilmsRatingDBContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=TheMovieDB;Username=postgres;Password=postgres");
            context = new FilmsRatingDBContext(builder.Options);
            controller = new UtilisateursController(context);
        }

        [TestMethod()]
        public void GetUtilisateursTest()
        {
            // Arrange
            List<Utilisateur> utilisateurs = context.Utilisateurs.ToList();

            // Act
            var result = controller.GetUtilisateurs();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<ActionResult<IEnumerable<Utilisateur>>>));
            CollectionAssert.AreEqual(utilisateurs, result.Result.Value.ToList(), "Les listes ne sont pas les mêmes");
        }

        [TestMethod()]
        public void GetUtilisateurByIdTest_ExistingIdPassed_ReturnsRightItem()
        {
            // Arrange
            int id = 1;
            Utilisateur utilisateur = context.Utilisateurs.Where(c => c.UtilisateurId == id).FirstOrDefault();
            // Act
            var result = controller.GetUtilisateurById(id);
            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<ActionResult<Utilisateur>>));
            Assert.AreEqual(utilisateur, result.Result.Value, "Les utilisateurs ne sont pas les mêmes");
        }

        [TestMethod()]
        public void GetUtilisateurByIdTest_UnknownIdPassed_ReturnsNotFoundResult()
        {
            // Arrange
            int id = -1;
            // Act
            var result = controller.GetUtilisateurById(id);
            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<ActionResult<Utilisateur>>));
            Assert.IsInstanceOfType(result.Result.Result, typeof(NotFoundResult), "Le résultat n'est pas NotFound");
        }

        [TestMethod()]
        public void GetUtilisateurByEmailTest_ExistingEmailPassed_ReturnsRightItem()
        {
            // Arrange
            string email = "clilleymd@last.fm";
            Utilisateur utilisateur = context.Utilisateurs.Where(c => c.Mail == email).FirstOrDefault();
            // Act
            var result = controller.GetUtilisateurByEmail(email);
            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<ActionResult<Utilisateur>>));
            Assert.AreEqual(utilisateur, result.Result.Value, "Les utilisateurs ne sont pas les mêmes");
        }

        [TestMethod()]
        public void GetUtilisateurByEmailTest_UnknownEmailPassed_ReturnsNotFoundResult()
        {
            // Arrange
            string email = "";
            // Act
            var result = controller.GetUtilisateurByEmail(email);
            // Assert
            Assert.IsInstanceOfType(result.Result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Postutilisateur_ModelValidated_CreationOK()
        {
            // Arrange
            Random rnd = new Random();
            int chiffre = rnd.Next(1, 1000000000);
            // Le mail doit être unique donc 2 possibilités :
            // 1. on s'arrange pour que le mail soit unique en concaténant un random ou un timestamp
            // 2. On supprime le user après l'avoir créé. Dans ce cas, nous avons besoin d'appeler la méthode DELETE de l’APIou remove du DbSet.
            Utilisateur userAtester = new Utilisateur()
            {
                Nom = "MACHIN",
                Prenom = "Luc",
                Mobile = "0606070809",
                Mail = "machin" + chiffre + "@gmail.com",
                Pwd = "Toto1234!",
                Rue = "Chemin de Bellevue",
                Cp = "74940",
                Ville = "Annecy-le-Vieux",
                Pays = "France",
                Latitude = null,
                Longitude = null
            };
            // Act
            var result = controller.PostUtilisateur(userAtester).Result; // .Result pour appeler la méthode async de manière synchrone, afin d'attendre l’ajout
            // Assert
            Utilisateur? userRecupere = context.Utilisateurs.Where(u => u.Mail.ToUpper() ==
            userAtester.Mail.ToUpper()).FirstOrDefault(); // On récupère l'utilisateur créé directement dans la BD grace à son mail unique
            // On ne connait pas l'ID de l’utilisateur envoyé car numéro automatique.
            // Du coup, on récupère l'ID de celui récupéré et on compare ensuite les 2 users
            userAtester.UtilisateurId = userRecupere.UtilisateurId;
                        Assert.AreEqual(userRecupere, userAtester, "Utilisateurs pas identiques");
        }
        [TestMethod]
        public void PutUtilisateur_ModelValidated_UpdateOK()
        {
            // ARRANGE
            // 1. On crée d'abord un utilisateur "original" dans la base pour avoir un ID valide
            Random rnd = new Random();
            int chiffre = rnd.Next(1, 1000000000);

            Utilisateur userOriginal = new Utilisateur()
            {
                Nom = "ORIGINAL",
                Prenom = "Test",
                Mobile = "0600000000",
                Mail = "original" + chiffre + "@gmail.com", // Mail unique
                Pwd = "Password123!",
                Rue = "Rue Test",
                Cp = "75000",
                Ville = "Paris",
                Pays = "France"
            };

            context.Utilisateurs.Add(userOriginal);
            context.SaveChanges(); // On sauvegarde pour générer l'ID (Identity)

            // 2. On prépare l'objet modifié (on change le Nom par exemple)
            // Important : On détache l'entité locale pour éviter les conflits de tracking EF Core lors du test
            context.Entry(userOriginal).State = EntityState.Detached;

            Utilisateur userModifie = new Utilisateur()
            {
                UtilisateurId = userOriginal.UtilisateurId, // Très important : même ID
                Nom = "MODIFIE", // La valeur qu'on veut changer
                Prenom = "Test",
                Mobile = "0600000000",
                Mail = userOriginal.Mail, // On garde le même mail
                Pwd = "Password123!",
                Rue = "Rue Test",
                Cp = "75000",
                Ville = "Paris",
                Pays = "France"
            };

            // ACT
            // Appel de la méthode PutUtilisateur du contrôleur
            var result = controller.PutUtilisateur(userModifie.UtilisateurId, userModifie).Result;

            // ASSERT
            // 1. Vérifier que le contrôleur retourne un code 204 (NoContent)
            Assert.IsInstanceOfType(result, typeof(NoContentResult), "Le contrôleur aurait dû renvoyer NoContent (204)");

            // 2. Vérifier en base de données que le nom a bien changé
            // On utilise AsNoTracking() pour être sûr de lire la vraie valeur en base et pas le cache
            Utilisateur userEnBase = context.Utilisateurs.AsNoTracking()
                                            .FirstOrDefault(u => u.UtilisateurId == userOriginal.UtilisateurId);

            Assert.AreEqual("MODIFIE", userEnBase.Nom, "Le nom n'a pas été mis à jour en base de données");
        }

        [TestMethod]
        public void DeleteUtilisateur_ExistingId_DeletesUser()
        {
            // ARRANGE
            // 1. Ajout d'un utilisateur via le DbSet (comme demandé dans le TP)
            Random rnd = new Random();
            int chiffre = rnd.Next(1, 1000000000);

            Utilisateur userToDelete = new Utilisateur()
            {
                Nom = "A_SUPPRIMER",
                Prenom = "Luc",
                Mobile = "0699999999",
                Mail = "delete" + chiffre + "@gmail.com",
                Pwd = "Delete123!",
                Rue = "Impasse",
                Cp = "00000",
                Ville = "Nowhere",
                Pays = "France"
            };

            context.Utilisateurs.Add(userToDelete);
            context.SaveChanges(); // Commit l'ajout pour avoir l'ID

            int idToDelete = userToDelete.UtilisateurId;

            // ACT
            // Appel de la méthode Delete du contrôleur
            var result = controller.DeleteUtilisateur(idToDelete).Result;

            // ASSERT
            // 1. Vérifier que le retour est bien un NoContent (ou Ok selon votre code, mais standard = NoContent)
            Assert.IsInstanceOfType(result, typeof(NoContentResult), "Le delete aurait dû renvoyer un NoContent");

            // 2. Vérification que l'utilisateur a été supprimé en le recherchant dans le DbSet
            Utilisateur userRecherche = context.Utilisateurs.Find(idToDelete);

            Assert.IsNull(userRecherche, "L'utilisateur existe toujours en base alors qu'il aurait dû être supprimé");
        }
    }
}