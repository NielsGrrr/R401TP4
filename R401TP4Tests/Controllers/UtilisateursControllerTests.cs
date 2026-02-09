using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using R401TP4.Controllers;
using R401TP4.Models.DataManager;
using R401TP4.Models.EntityFramework;
using R401TP4.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R401TP4.Controllers.Tests
{
    [TestClass()]
    public class UtilisateursControllerTests
    {
        private UtilisateursController controller;
        private FilmsRatingDBContext context;
        private IDataRepository<Utilisateur> dataRepository;

        [TestInitialize()]
        public void TestInitialize()
        {
            var builder = new DbContextOptionsBuilder<FilmsRatingDBContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=TheMovieDB;Username=postgres;Password=postgres");
            context = new FilmsRatingDBContext(builder.Options);
            dataRepository = new UtilisateurManager(context);
            controller = new UtilisateursController(dataRepository);
        }

        [TestMethod()]
        public async Task GetUtilisateursTest()
        {
            // Arrange
            List<Utilisateur> utilisateurs = context.Utilisateurs.ToList();

            // Act
            var result = await controller.GetUtilisateurs();

            // Assert
            // CORRECTION : Avec await, ce n'est plus une Task, mais directement un ActionResult
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Utilisateur>>));

            // On vérifie le contenu
            CollectionAssert.AreEqual(utilisateurs, result.Value.ToList(), "Les listes ne sont pas les mêmes");
        }

        [TestMethod()]
        public async Task GetUtilisateurByIdTest_ExistingIdPassed_ReturnsRightItem()
        {
            // Arrange
            int id = 1;
            Utilisateur utilisateur = context.Utilisateurs.Where(c => c.UtilisateurId == id).FirstOrDefault();

            // Act
            var result = await controller.GetUtilisateurById(id);

            // Assert
            // CORRECTION : On vérifie que c'est bien un ActionResult<Utilisateur>
            Assert.IsInstanceOfType(result, typeof(ActionResult<Utilisateur>));
            Assert.AreEqual(utilisateur.UtilisateurId, result.Value.UtilisateurId, "Les utilisateurs ne sont pas les mêmes");
        }

        [TestMethod()]
        public async Task GetUtilisateurByIdTest_UnknownIdPassed_ReturnsNotFoundResult()
        {
            // Arrange
            int id = -1;

            // Act
            var result = await controller.GetUtilisateurById(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Utilisateur>));
            // Ici on vérifie que le Result interne est bien un NotFoundResult
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult), "Le résultat n'est pas NotFound");
        }

        [TestMethod()]
        public async Task GetUtilisateurByEmailTest_ExistingEmailPassed_ReturnsRightItem()
        {
            // Arrange
            string email = "clilleymd@last.fm";
            Utilisateur utilisateur = context.Utilisateurs.Where(c => c.Mail == email).FirstOrDefault();

            // Act
            var result = await controller.GetUtilisateurByEmail(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Utilisateur>));
            Assert.AreEqual(utilisateur.Mail, result.Value.Mail, "Les utilisateurs ne sont pas les mêmes");
        }

        [TestMethod()]
        public async Task GetUtilisateurByEmailTest_UnknownEmailPassed_ReturnsNotFoundResult()
        {
            // Arrange
            string email = "inconnu@test.com";

            // Act
            var result = await controller.GetUtilisateurByEmail(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Utilisateur>));
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Postutilisateur_ModelValidated_CreationOK()
        {
            // Arrange
            Random rnd = new Random();
            int chiffre = rnd.Next(1, 1000000000);

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
            // CORRECTION : On utilise await ici aussi
            var actionResult = await controller.PostUtilisateur(userAtester);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ActionResult<Utilisateur>));

            // On vérifie que c'est un CreatedAtActionResult (code 201)
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            var createdResult = actionResult.Result as CreatedAtActionResult;
            Assert.AreEqual(201, createdResult.StatusCode);

            // Vérification en base
            Utilisateur userRecupere = context.Utilisateurs.Where(u => u.Mail.ToUpper() == userAtester.Mail.ToUpper()).FirstOrDefault();
            Assert.IsNotNull(userRecupere);
        }

        [TestMethod]
        public async Task PutUtilisateur_ModelValidated_UpdateOK()
        {
            // ARRANGE
            Random rnd = new Random();
            int chiffre = rnd.Next(1, 1000000000);

            Utilisateur userOriginal = new Utilisateur()
            {
                Nom = "ORIGINAL",
                Prenom = "Test",
                Mobile = "0600000000",
                Mail = "original" + chiffre + "@gmail.com",
                Pwd = "Password123!",
                Rue = "Rue Test",
                Cp = "75000",
                Ville = "Paris",
                Pays = "France"
            };

            context.Utilisateurs.Add(userOriginal);
            context.SaveChanges();

            // Détacher
            context.Entry(userOriginal).State = EntityState.Detached;

            Utilisateur userModifie = new Utilisateur()
            {
                UtilisateurId = userOriginal.UtilisateurId,
                Nom = "MODIFIE",
                Prenom = "Test",
                Mobile = "0600000000",
                Mail = userOriginal.Mail,
                Pwd = "Password123!",
                Rue = "Rue Test",
                Cp = "75000",
                Ville = "Paris",
                Pays = "France"
            };

            // ACT
            // CORRECTION : await au lieu de .Result
            var result = await controller.PutUtilisateur(userModifie.UtilisateurId, userModifie);

            // ASSERT
            // Pour le PUT, le contrôleur retourne IActionResult, qui est implémenté par NoContentResult
            Assert.IsInstanceOfType(result, typeof(NoContentResult), "Le contrôleur aurait dû renvoyer NoContent (204)");

            // Vérification
            Utilisateur userEnBase = context.Utilisateurs.AsNoTracking()
                                            .FirstOrDefault(u => u.UtilisateurId == userOriginal.UtilisateurId);
            Assert.AreEqual("MODIFIE", userEnBase.Nom);
        }

        [TestMethod]
        public async Task DeleteUtilisateur_ExistingId_DeletesUser()
        {
            // ARRANGE
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
            context.SaveChanges();
            int idToDelete = userToDelete.UtilisateurId;

            // Détacher pour éviter les conflits dans le Repository
            context.Entry(userToDelete).State = EntityState.Detached;

            // ACT
            // CORRECTION : await au lieu de .Result
            var result = await controller.DeleteUtilisateur(idToDelete);

            // ASSERT
            Assert.IsInstanceOfType(result, typeof(NoContentResult), "Le delete aurait dû renvoyer un NoContent");

            Utilisateur userRecherche = context.Utilisateurs.Find(idToDelete);
            Assert.IsNull(userRecherche, "L'utilisateur existe toujours en base");
        }
    }
}