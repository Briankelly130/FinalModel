using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlineStore.Domain.Abstract;
using OnlineStore.Domain.Entities;
using OnlineStore.WebUI.Controllers;
using OnlineStore.WebUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineStore.WebUI.HtmlHelpers;


namespace OnlineStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            //Arrange
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"},
                new Game {GameID = 3, Name = "P3"},
                new Game {GameID = 4, Name = "P4"},
                new Game {GameID = 5, Name = "P5"}
            }.AsQueryable());

            GameController controller = new GameController(mock.Object);
            controller.PageSize = 3;

            //Act
            GamesListViewModel result = (GamesListViewModel)controller.List(null, 2).Model;

            //Assert
            Game[] gameArray = result.Games.ToArray();
            Assert.IsTrue(gameArray.Length == 2);
            Assert.AreEqual(gameArray[0].Name, "P4");
            Assert.AreEqual(gameArray[1].Name, "P5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            //Arrange - define an HTML Helper - we need to do this
            // in order to apply the extension method
            HtmlHelper myHelper = null;

            //Arrange - Create PagingInfo data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            //Arrange - set up the delegate using a lambda expression
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual(result.ToString(), @"<a href=""Page 1"">1</a>"
                + @"<a class =""selected"" href=""Page 2"">2</a>"
                + @"<a href=""Page 3"">3</a>");
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            //Arrange
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1"},
                new Game {GameID = 2, Name = "P2"},
                new Game {GameID = 3, Name = "P3"},
                new Game {GameID = 4, Name = "P4"},
                new Game {GameID = 5, Name = "P5"}
            }.AsQueryable());

            GameController controller = new GameController(mock.Object);
            controller.PageSize = 3;

            //Act
            GamesListViewModel result = (GamesListViewModel)controller.List(2).Model;

            //Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Games()
        {
            //Arrange
            // - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Category = "Cat1"},
                new Game {GameID = 2, Category = "Cat2"},
                new Game {GameID = 3, Category = "Cat1"},
                new Game {GameID = 4, Category = "Cat2"},
                new Game {GameID = 5, Category = "Cat3"}
            }.AsQueryable());

            //Arrange - create a controller and make the page size 3 times
            GameController controller = new GameController(mock.Object);

            controller.PageSize = 3;

            //Action
            Game[] result = ((GamesListViewModel)controller.List("Cat2", 1).Model)
                .Games.ToArray();

            //Assert
            Assert.AreEqual(result.Length, 2);
            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "Cat2");
            Assert.IsTrue(result[1].Name == "P4" && result[1].Category == "Cat2");
        }

        [TestMethod]
        public void Can_Create_Categories()
        {
            //Arrange
            // - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1", Category = "Apples"},
                new Game {GameID = 2, Name = "P2", Category = "Apples"},
                new Game {GameID = 3, Name = "P3", Category = "Plums"},
                new Game {GameID = 4, Name = "P4", Category = "Oranges"},
            }.AsQueryable());

            // Arrange - Create the controller
            NavController target = new NavController(mock.Object);

            //Act - Get the set of categories
            string[] results = ((IEnumerable<string>)target.Menu().Model).ToArray();

            //Assert
            Assert.AreEqual(results.Length, 3);
            Assert.AreEqual(results[0], "Apples");
            Assert.AreEqual(results[1], "Oranges");
            Assert.AreEqual(results[2], "Plums");
        }

        [TestMethod]
        public void Indicates_Selected_Category()
        {
            //Arrange
            // - create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1", Category = "Apples"},
                new Game {GameID = 4, Name = "P2", Category = "Oranges"},
            }.AsQueryable());

            // Arrange - Create the controller
            NavController target = new NavController(mock.Object);

            // Arrange - Define the category to select
            string categoryToSelect = "Apples";

            // Action
            string result = target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            //Assert
            Assert.AreEqual(categoryToSelect, result);
        }

        [TestMethod]
        public void Generate_Category_Specific_Game_Count()
        {
            // Arrange
            // - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1", Category = "Cat1"},
                new Game {GameID = 2, Name = "P2", Category = "Cat2"},
                new Game {GameID = 3, Name = "P3", Category = "Cat1"},
                new Game {GameID = 4, Name = "P4", Category = "Cat2"},
                new Game {GameID = 5, Name = "P5", Category = "Cat3"},
            }.AsQueryable());

            // Arrange - Create a controller and make the page size 3 times
            GameController target = new GameController(mock.Object);
            target.PageSize = 3;

            // Action - test the game counts for different categories
            int res1 = ((GamesListViewModel)target.List("Cat1").Model).PagingInfo.TotalItems;
            int res2 = ((GamesListViewModel)target.List("Cat2").Model).PagingInfo.TotalItems;
            int res3 = ((GamesListViewModel)target.List("Cat3").Model).PagingInfo.TotalItems;
            int resAll = ((GamesListViewModel)target.List(null).Model).PagingInfo.TotalItems;

            //Assert
            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
            Assert.AreEqual(resAll, 5);
        }
    }
}
