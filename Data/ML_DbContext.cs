using Microsoft.EntityFrameworkCore;
using MokkilicoresExpressAPI.Models;

namespace MokkilicoresExpressAPI.Data
{
	public class ML_DbContext: DbContext
	{
		public ML_DbContext(DbContextOptions<ML_DbContext> options): base(options)
		{
        }
            public DbSet<MokkilicoresExpressAPI.Models.Cliente> Cliente { get; set; }
            public DbSet<MokkilicoresExpressAPI.Models.Inventario> Inventario { get; set; }
            public DbSet<MokkilicoresExpressAPI.Models.Direccion> Direccion { get; set; }
            public DbSet<MokkilicoresExpressAPI.Models.Pedido> Pedido { get; set; }
            //public DbSet<MokkilicoresExpressAPI.Models.LoginRequest> LoginRequest { get; set; }
    }
}
