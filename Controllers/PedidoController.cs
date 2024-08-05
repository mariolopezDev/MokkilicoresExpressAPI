using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MokkilicoresExpressAPI.Data;
using MokkilicoresExpressAPI.Models;
using System.Security.Claims;
using System.Text.Json;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly ML_DbContext _context;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(ML_DbContext context, ILogger<PedidoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> Get()
        {
            return await _context.Pedido.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> Get(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            _logger.LogDebug("Get Pedido llamado con id {PedidoId}. Pedido: {pedido}", id, pedido);
            return Ok(pedido);
        }

        [HttpGet("Cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetByCliente(int clienteId)
        {
            var pedidosCliente = await _context.Pedido.Where(p => p.ClienteId == clienteId).ToListAsync();
            if (pedidosCliente == null || pedidosCliente.Count == 0)
            {
                _logger.LogWarning("Pedidos for client with id {ClienteId} not found.", clienteId);
                return NotFound();
            }

            _logger.LogDebug("Get Pedidos for client with id {ClienteId}. Pedidos: {pedidosCliente}", clienteId, pedidosCliente);
            return Ok(pedidosCliente);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Pedido pedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos: {PedidoData}", JsonSerializer.Serialize(pedido));

            // Load related data
            var cliente = await _context.Cliente.FindAsync(pedido.ClienteId);
            var inventario = await _context.Inventario.FindAsync(pedido.InventarioId);
            var direccion = await _context.Direccion.FirstOrDefaultAsync(d => d.Id == pedido.DireccionId && d.ClienteId == pedido.ClienteId);

            if (cliente == null || inventario == null || direccion == null)
            {
                _logger.LogError("Error: Cliente, Inventario o Dirección no encontrados.");
                return BadRequest("Cliente, Inventario o Dirección no encontrados.");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (pedido.ClienteId != cliente.Id)
                return Unauthorized();

            if (inventario == null)
            {
                _logger.LogError("Error: Inventario no encontrado.");
                return BadRequest("Inventario no encontrado.");
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

            pedido.CostoSinIVA = inventario.Precio * pedido.Cantidad;
            pedido.CostoTotal = pedido.CostoSinIVA * 1.13M; // Calcular el costo total

            _context.Pedido.Add(pedido);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido creado exitosamente con id {PedidoId}.", pedido.Id);
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Pedido updatedPedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos para actualización: {PedidoData}", JsonSerializer.Serialize(updatedPedido));

            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            var inventario = await _context.Inventario.FindAsync(updatedPedido.InventarioId);
            var cliente = await _context.Cliente.FindAsync(updatedPedido.ClienteId);

            if (cliente == null || inventario == null)
            {
                _logger.LogError("Error: Cliente o Inventario no encontrados.");
                return BadRequest(new { Message = "Cliente o Inventario no encontrados." });
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (pedido.ClienteId != cliente.Id)
                return Unauthorized();

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

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido actualizado exitosamente con id {PedidoId}.", id);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            _context.Pedido.Remove(pedido);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pedido eliminado exitosamente con id {PedidoId}.", id);
            return NoContent();
        }
    }
}
