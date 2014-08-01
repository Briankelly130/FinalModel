using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineStore.Domain.Abstract;
using OnlineStore.Domain.Entities;
using OnlineStore.WebUI.Models;

namespace OnlineStore.WebUI.Controllers
{
    public class GameController : Controller
    {
        private IGameRepository repository;
        public int PageSize = 4;

        public GameController(IGameRepository gameRepository)
        {
            this.repository = gameRepository;
        }

        public ViewResult List (string category, int page = 1)
        {
            GamesListViewModel model = new GamesListViewModel
            {
                Games = repository.Games
                .Where(p => category == null || p.Category == category)
                .OrderBy(p => p.GameID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = category == null ?
                        repository.Games.Count() :
                        repository.Games.Where(e => e.Category == category).Count()
                },
                CurrentCategory = category
            };
            return View(model);
        }

        public FileContentResult GetImage(int gameId)
        {
            Game game = repository.Games.FirstOrDefault(p => p.GameID == gameId);
            if (game != null)
            {
                return File(game.ImageData, game.ImageMimeType);
            } else {
                return null;
            }
        }
    }
}
