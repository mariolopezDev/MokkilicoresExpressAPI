using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Data;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ML_DbContext _context;

        public ClienteController(ML_DbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> Get()
        {
            return await _context.Cliente.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> Get(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null)
                return NotFound(new { Message = $"Cliente con ID {id} no encontrado" });
            return Ok(cliente);
        }

        [HttpGet("Usuario/{identificacion}")]
        public async Task<ActionResult<Cliente>> Get(string identificacion)
        {
            var cliente = await _context.Cliente.FirstOrDefaultAsync(c => c.Identificacion == identificacion);
            if (cliente == null)
                return NotFound(new { Message = $"Cliente con identificación {identificacion} no encontrado" });
            return Ok(cliente);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Cliente cliente)
        {
            if (cliente == null || !ModelState.IsValid)
            {
                return BadRequest(new { Message = "Datos de cliente no válidos o incompletos" });
            }
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = cliente.Id }, new { Message = "Cliente creado exitosamente", cliente });
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Cliente updatedCliente)
        {
            if (updatedCliente == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de cliente no válidos.");
            }
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null)
                return NotFound();

            cliente.Nombre = updatedCliente.Nombre;
            cliente.Apellido = updatedCliente.Apellido;
            cliente.DineroCompradoTotal = updatedCliente.DineroCompradoTotal;
            cliente.DineroCompradoUltimoAno = updatedCliente.DineroCompradoUltimoAno;
            cliente.DineroCompradoUltimos6Meses = updatedCliente.DineroCompradoUltimos6Meses;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null)
                return NotFound();

            _context.Cliente.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
