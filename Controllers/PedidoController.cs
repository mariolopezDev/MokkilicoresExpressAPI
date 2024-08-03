using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PedidoController> _logger;
        private const string CacheKey = "Pedidos";
        private const string ClientesCacheKey = "Clientes";
        private const string InventariosCacheKey = "Inventarios";
        private const string DireccionesCacheKey = "Direcciones";

        public PedidoController(IMemoryCache cache, ILogger<PedidoController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Pedido>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Pedido> pedidos))
            {
                pedidos = new List<Pedido>();
                _cache.Set(CacheKey, pedidos);
            }

            _logger.LogDebug("Get Pedidos llamado. Pedidos: {pedidos}", pedidos);
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public ActionResult<Pedido> Get(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            _logger.LogDebug("Get Pedido llamado con id {PedidoId}. Pedido: {pedido}", id, pedido);
            return Ok(pedido);
        }

        [HttpGet("Cliente/{clienteId}")]
        public ActionResult<IEnumerable<Pedido>> GetByCliente(int clienteId)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedidosCliente = pedidos?.Where(p => p.ClienteId == clienteId).ToList();
            if (pedidosCliente == null || pedidosCliente.Count == 0)
            {
                _logger.LogWarning("Pedidos for client with id {ClienteId} not found.", clienteId);
                return NotFound();
            }

            _logger.LogDebug("Get Pedidos for client with id {ClienteId}. Pedidos: {pedidosCliente}", clienteId, pedidosCliente);
            return Ok(pedidosCliente);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Pedido pedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos: {PedidoData}", JsonSerializer.Serialize(pedido));

            

            // Load related data
            var clientes = _cache.Get<List<Cliente>>(ClientesCacheKey) ?? new List<Cliente>();
            var inventarios = _cache.Get<List<Inventario>>(InventariosCacheKey) ?? new List<Inventario>();
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey) ?? new List<Direccion>();
            
            var pedidos = _cache.Get<List<Pedido>>(CacheKey) ?? new List<Pedido>();
            
            pedido.Id = pedidos.Count > 0 ? pedidos.Max(p => p.Id) + 1 : 1;
            pedido.CostoSinIVA = inventarios.FirstOrDefault(i => i.Id == pedido.InventarioId)?.Precio * pedido.Cantidad ?? 0;
            pedido.CostoTotal = pedido.CostoSinIVA * 1.13M; // Calcular el costo total

            var cliente = clientes.FirstOrDefault(c => c.Id == pedido.ClienteId);
            var inventario = inventarios.FirstOrDefault(i => i.Id == pedido.InventarioId);
            var direccion = direcciones.FirstOrDefault(d => d.Id == pedido.DireccionId && d.ClienteId == pedido.ClienteId);


            if (inventario == null)
            {
                _logger.LogError("Error: Cliente o Inventario no encontrados.");
                return BadRequest("Cliente o Inventario no encontrados.");
            }
            if (direccion == null)
            {
                _logger.LogError("Error: Dirección no encontrada o no pertenece al cliente.");
                return BadRequest("Dirección no válida para este cliente.");
            }
            if (cliente == null)
            {
                _logger.LogError("Error: Cliente no encontrado.");
                return BadRequest("Cliente no encontrado.");
            }


            pedidos.Add(pedido);
            _logger.LogInformation("Pedido creado exitosamente con id {PedidoId}.", pedido.Id);
            _cache.Set(CacheKey, pedidos);
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Pedido updatedPedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos para actualización: {PedidoData}", JsonSerializer.Serialize(updatedPedido));

            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            
            var inventario = _cache.Get<List<Inventario>>(InventariosCacheKey)?.FirstOrDefault(i => i.Id == updatedPedido.InventarioId);
            var cliente = _cache.Get<List<Cliente>>(ClientesCacheKey)?.FirstOrDefault(c => c.Id == updatedPedido.ClienteId);

            if (cliente == null || inventario == null)
            {
                _logger.LogError("Error: Cliente o Inventario no encontrados.");
                return BadRequest(new { Message = "Cliente o Inventario no encontrados." });
            }

            _logger.LogInformation("updatedPedido.CostoSinIVA {CostoSinIVA}", updatedPedido.CostoSinIVA);
            _logger.LogInformation("inventario.Precio {Precio}", inventario.Precio);
            _logger.LogInformation("updatedPedido.Cantidad {Cantidad}", updatedPedido.Cantidad);
            _logger.LogInformation("inventario.Precio * updatedPedido.Cantidad {PrecioCantidad}", inventario.Precio * updatedPedido.Cantidad);

            
           
            // Actualizar pedido con los nuevos datos recibidos
            pedido.ClienteId = updatedPedido.ClienteId;
            pedido.InventarioId = updatedPedido.InventarioId;
            pedido.Cantidad = updatedPedido.Cantidad;
            pedido.CostoSinIVA = inventario.Precio * updatedPedido.Cantidad;

            _logger.LogInformation("Pedido.CostoSinIVA {CostoSinIVA}", pedido.CostoSinIVA);

            //Calclulo del % de descuento
            if (pedido.CostoSinIVA > 50000)
            {
                pedido.PorcentajeDescuento = 0.2M;
            }
            else if (pedido.CostoSinIVA > 25000)
            {
                pedido.PorcentajeDescuento = 0.1M;
            }
            else
            {
                pedido.PorcentajeDescuento = 0;
            }
            var MontoDescuento = pedido.CostoSinIVA * pedido.PorcentajeDescuento;
            _logger.LogInformation("MontoDescuento {MontoDescuento}", MontoDescuento);

            pedido.CostoTotal = (pedido.CostoSinIVA - MontoDescuento) * 1.13M;
            _logger.LogInformation("Pedido.CostoTotal {CostoTotal}", pedido.CostoTotal);
            pedido.Estado = updatedPedido.Estado;

            // Actualizar Historial de compras del cliente
            cliente.DineroCompradoTotal += pedido.CostoTotal;
                // TODO: Pendiente logica para tiempos de compra
            cliente.DineroCompradoUltimos6Meses += pedido.CostoTotal;
            cliente.DineroCompradoUltimoAno += pedido.CostoTotal;

            _logger.LogInformation("Pedido actualizado exitosamente con id {PedidoId}.", id);
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            pedidos.Remove(pedido);
            _logger.LogInformation("Pedido eliminado exitosamente con id {PedidoId}.", id);
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }
    }
}
