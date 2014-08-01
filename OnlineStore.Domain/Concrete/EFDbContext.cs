using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineStore.Domain.Entities;
using System.Data.Entity;

namespace OnlineStore.Domain.Concrete
{
    public class EFDbContext :DbContext
    {
        public DbSet<Game> Games { get; set; }
    }
}
