namespace MokkilicoresExpressAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        //public Cliente? Cliente { get; set; } // No obligatorio para la creación
        public int InventarioId { get; set; }
        //public Inventario? Inventario { get; set; } // No obligatorio para la creación
        public int Cantidad { get; set; }
        public decimal CostoSinIVA { get; set; }
        public decimal CostoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
