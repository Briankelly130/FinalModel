using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlineStore.Domain.Abstract;
using OnlineStore.Domain.Entities;
using OnlineStore.WebUI.Controllers;
using System.Linq;
using System.Web.Mvc;

namespace OnlineStore.UnitTests
{
    [TestClass]
    public class ImageTests
    {
        [TestMethod]
        public void Can_Retrieve_Image_Data()
        {
            // Arrange - Create a game with image data
            Game game = new Game
            {
                GameID = 2,
                Name = "Test",
                ImageData = new byte[] { },
                ImageMimeType = "image/png"
            };

            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                game,
                new Game {GameID = 3, Name = "P3"}
            }.AsQueryable());

            // Arrange - Create the controller
            GameController target = new GameController(mock.Object);

            // Act - Call the GetImage action method
            ActionResult result = target.GetImage(2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FileResult));
            Assert.AreEqual(game.ImageMimeType, ((FileResult)result).ContentType);
        }

        [TestMethod]
        public void Cannot_Retrieve_Image_Data_For_Invalid_ID()
        {
            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"}
            }.AsQueryable());

            // Arrange - Create the controller
            GameController target = new GameController(mock.Object);

            // Act - Call the GetImage action method
            ActionResult result = target.GetImage(100);

            // Assert
            Assert.IsNull(result);
        }
    }
}
