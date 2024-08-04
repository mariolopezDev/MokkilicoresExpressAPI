using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MokkilicoresExpressAPI.Data;
using MokkilicoresExpressAPI.Models;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly ML_DbContext _context;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(ML_DbContext context, ILogger<InventarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventario>>> Get()
        {
            return await _context.Inventario.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventario>> Get(int id)
        {
            var inventario = await _context.Inventario.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });
            return Ok(inventario);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Inventario inventario)
        {
            _logger.LogInformation($"Recibida solicitud POST para crear inventario: {System.Text.Json.JsonSerializer.Serialize(inventario)}");
            if (inventario == null)
            {
                return BadRequest(new { Message = "Inventario no puede ser nulo" });
            }

            if (string.IsNullOrEmpty(inventario.TipoLicor))
            {
                return BadRequest(new { Message = "Tipo de licor es un campo requerido" });
            }
            if (inventario.CantidadEnExistencia <= 0)
            {
                return BadRequest(new { Message = "Cantidad en existencia debe ser mayor a 0" });
            }
            if (inventario.Precio <= 0)
            {
                return BadRequest(new { Message = "Precio debe ser mayor a 0" });
            }

            _context.Inventario.Add(inventario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = inventario.Id }, inventario);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Inventario updatedInventario)
        {
            if (updatedInventario == null)
                return BadRequest(new { Message = "Inventario no puede ser nulo" });

            var inventario = await _context.Inventario.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            if (string.IsNullOrEmpty(updatedInventario.TipoLicor))
                return BadRequest(new { Message = "Tipo de licor es un campo requerido" });

            inventario.CantidadEnExistencia = updatedInventario.CantidadEnExistencia;
            inventario.BodegaId = updatedInventario.BodegaId;
            inventario.Precio = updatedInventario.Precio;
            inventario.FechaIngreso = updatedInventario.FechaIngreso;
            inventario.FechaVencimiento = updatedInventario.FechaVencimiento;
            inventario.TipoLicor = updatedInventario.TipoLicor;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var inventario = await _context.Inventario.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            _context.Inventario.Remove(inventario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
