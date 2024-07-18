namespace MokkilicoresExpressAPI.Models
{
    public class Direccion
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string PuntoEnWaze { get; set; } = string.Empty;
        public bool EsCondominio { get; set; }
        public bool EsPrincipal { get; set; }
    }
}
