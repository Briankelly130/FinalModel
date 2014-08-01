using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineStore.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using OnlineStore.Domain.Abstract;
using OnlineStore.WebUI.Controllers;
using System.Web.Mvc;
using OnlineStore.WebUI.Models;

namespace OnlineStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // Arrange - Create some test games
            Game p1 = new Game { GameID = 1, Name = "P1" };
            Game p2 = new Game { GameID = 2, Name = "P2" };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] results = target.Lines.ToArray();

            // Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Game, p1);
            Assert.AreEqual(results[1].Game, p2);
        }

        [TestMethod]
        public void Can_Add_Quality_For_Existing_Lines()
        {
            //Arrange - Create some test games
            Game p1 = new Game { GameID = 1, Name = "P1" };
            Game p2 = new Game { GameID = 2, Name = "P2" };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] results = target.Lines.OrderBy(c => c.Game.GameID).ToArray();

            // Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Quantity, p1);
            Assert.AreEqual(results[1].Quantity, p2);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            // Arrange - Create some test games
            Game p1 = new Game { GameID = 1, Name = "P1" };
            Game p2 = new Game { GameID = 2, Name = "P2" };
            Game p3 = new Game { GameID = 3, Name = "P3" };

            // Arrange - Create a new cart
            Cart target = new Cart();
            // Arrange - add some games to the cart
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            // Act
            target.RemoveLine(p2);

            // Assert
            Assert.AreEqual(target.Lines.Where(c => c.Game == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // Arrange - Create some test games
            Game p1 = new Game { GameID = 1, Name = "P1", Price = 100M };
            Game p2 = new Game { GameID = 2, Name = "P2", Price = 50M };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();

            // Assert
            Assert.AreEqual(result, 450M);
        }

        [TestMethod]

        public void Can_Clear_Contents()
        {
            // Arrange - Create some test games
            Game p1 = new Game { GameID = 1, Name = "P1", Price = 100M };
            Game p2 = new Game { GameID = 2, Name = "P2", Price = 50M };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Arrange - Add some items
            target.AddItem(p1, 1);
            target.AddItem(p2, 2);

            // Act - Reset the cart
            target.Clear();

            // Assert
            Assert.AreEqual(target.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // Arrange - Create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());

            // Arrange - Create a new cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(mock.Object, null);

            // Act - Add a game to the cart
            target.AddToCart(cart, 1, null);

            // Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Game.GameID, 1);
        }

        [TestMethod]
        public void Adding_Game_To_Cart_Goes_To_Cart_Screen()
        {
            // Arrange - create the mock repository
            Mock<IGameRepository> mock = new Mock<IGameRepository>();
            mock.Setup(m => m.Games).Returns(new Game[]
            {
                new Game {GameID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());

            // Arrange - Create a new cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(mock.Object, null);

            // Act - Add a game to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            // Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // Arrange - Create a cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(null, null);

            //Act - Call the Index action method
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // Assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // Arrange - Create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - Create an empty cart
            Cart cart = new Cart();

            // Arrange - Create shipping details
            ShippingDetails shippingDetails = new ShippingDetails();

            // Arrange - Create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Act
            ViewResult result = target.Checkout(cart, shippingDetails);

            // Assert - Check that the order hasn't been passed onto the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());
            // Assert - Check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);
            // Assert - Check that we are passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            // Arrange - Create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - Create a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Game(), 1);

            // Arrange - Create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Arrange - Add an error to the model
            target.ModelState.AddModelError("error", "error");

            // Act - Try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert - Check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());
            // Assert - Check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);
            // Assert - Check that we are passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            // Arrange - Create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - Create a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Game(), 1);

            // Arrange - Create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Act - Try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert - Check that the order has been passed onto the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Once());

            // Assert - Check that the method is returning the Completed view
            Assert.AreEqual("Completed", result.ViewName);

            // Assert - Check that we are passing a valid model to the view
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }

    }
}
