using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlineStore.Domain.Abstract;
using OnlineStore.Domain.Entities;
using OnlineStore.WebUI.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnlineStore.UnitTests
{
    [TestClass]
    public class AdminTests
    {
        [TestMethod]
        public void Index_Contains_All_Games()
        {
            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"},
                new Game {GameID = 3, Name = "P3"},
            }.AsQueryable());

            // Arrange - Create a controller
            AdminController target = new AdminController(mock.Object);

            // Action
            Game[] result = ((IEnumerable<Game>)target.Index().
                ViewData.Model).ToArray();

            // Assert
            Assert.AreEqual(result.Length, 3);
            Assert.AreEqual("P1", result[0].Name);
            Assert.AreEqual("P2", result[1].Name);
            Assert.AreEqual("P3", result[2].Name);
        }

        [TestMethod]
        public void Can_Edit_Game()
        {
            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"},
                new Game {GameID = 3, Name = "P3"},
            }.AsQueryable());

            // Arrange - Create the controller
            AdminController target = new AdminController(mock.Object);

            // Act
            Game p1 = target.Edit(1).ViewData.Model as Game;
            Game p2 = target.Edit(2).ViewData.Model as Game;
            Game p3 = target.Edit(3).ViewData.Model as Game;

            // Assert
            Assert.AreEqual(1, p1.GameID);
            Assert.AreEqual(2, p2.GameID);
            Assert.AreEqual(3, p3.GameID);
        }

        [TestMethod]
        public void Cannot_Edit_Nonexistent_Game()
        {
            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"},
                new Game {GameID = 3, Name = "P3"},
            }.AsQueryable());

            // Arrange - Create the controller
            AdminController target = new AdminController(mock.Object);

            // Act
            Game result = (Game)target.Edit(4).ViewData.Model;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Can_Save_Valid_Changes()
        {
            // Arrange - Create mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();

            // Arrange - Create the controller
            AdminController target = new AdminController(mock.Object);

            // Arrange - Create a game
            Game game = new Game { Name = "Test" };

            // Act - Try to save the game
            ActionResult result = target.Edit(game);

            // Assert - Check that the repository was called
            mock.Verify(m => m.SaveGame(game));

            // Assert - Check the method result type
            Assert.IsNotInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Cannot_Save_Invalid_Changes()
        {
            // Arrange - Create mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();

            // Arrange - Create the controller
            AdminController target = new AdminController(mock.Object);

            // Arrange - Create a game
            Game game = new Game { Name = "Test" };

            // Arrange - Add an error to the model state
            target.ModelState.AddModelError("error", "error");

            // Act - Try to save the game
            ActionResult result = target.Edit(game);

            // Assert - Check that the repository was not called
            mock.Verify(m => m.SaveGame(It.IsAny<Game>()), Times.Never());

            // Assert - Check the method result type
            Assert.IsNotInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Can_Delete_Valid_Games()
        {
            // Arrange - Create a Game
            Game game = new Game { GameID = 2, Name = "Test" };

            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                game,
                new Game {GameID = 3, Name = "P3"},
            }.AsQueryable());

            // Arrange - Create the controller
            AdminController target = new AdminController(mock.Object);

            // Act - Delete the Game
            target.Delete(game.GameID);

            // Assert - Ensure that the repository delete method was
            // called with the correct Game
            mock.Verify(m => m.DeleteGame(game.GameID));
        }
    }
}
