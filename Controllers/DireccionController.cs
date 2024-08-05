using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Data;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionController : ControllerBase
    {
        private readonly ML_DbContext _context;

        public DireccionController(ML_DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Direccion>>> Get()
        {
            return await _context.Direccion.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Direccion>> Get(int id)
        {
            var direccion = await _context.Direccion.FindAsync(id);
            if (direccion == null)
                return NotFound();
            return Ok(direccion);
        }

        [HttpGet("Usuario/{identificacion}")]
        public async Task<ActionResult<IEnumerable<Direccion>>> GetDireccionesByUsuario(string identificacion)
        {
            var cliente = await _context.Cliente.FirstOrDefaultAsync(c => c.Identificacion == identificacion);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado");
            }

            var direccionesCliente = await _context.Direccion
                .Where(d => d.ClienteId == cliente.Id)
                .Select(d => new Direccion
                {
                    Id = d.Id,
                    ClienteId = d.ClienteId,
                    Provincia = d.Provincia,
                    Canton = d.Canton,
                    Distrito = d.Distrito,
                    PuntoEnWaze = d.PuntoEnWaze,
                    EsCondominio = d.EsCondominio,
                    EsPrincipal = d.EsPrincipal,
                    DireccionCompleta = $"{d.Id} - {d.Provincia}, {d.Canton}, {d.Distrito}"
                })
                .ToListAsync();

            return Ok(direccionesCliente);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Direccion direccion)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (direccion.ClienteId != cliente.Id)
                return Unauthorized();

            _context.Direccion.Add(direccion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = direccion.Id }, direccion);
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Direccion updatedDireccion)
        {
            var direccion = await _context.Direccion.FindAsync(id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            direccion.ClienteId = updatedDireccion.ClienteId;
            direccion.Provincia = updatedDireccion.Provincia;
            direccion.Canton = updatedDireccion.Canton;
            direccion.Distrito = updatedDireccion.Distrito;
            direccion.PuntoEnWaze = updatedDireccion.PuntoEnWaze;
            direccion.EsCondominio = updatedDireccion.EsCondominio;
            direccion.EsPrincipal = updatedDireccion.EsPrincipal;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var direccion = await _context.Direccion.FindAsync(id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            _context.Direccion.Remove(direccion);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Dirección eliminada correctamente." });
        }

        [HttpGet("Cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Direccion>>> GetDireccionesPorCliente(int clienteId)
        {
            var direcciones = await _context.Direccion.Where(d => d.ClienteId == clienteId).ToListAsync();
            return Ok(direcciones);
        }
    }
}
