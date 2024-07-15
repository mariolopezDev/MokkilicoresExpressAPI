namespace MokkilicoresExpressAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoSinIVA { get; set; }
        public decimal CostoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
