using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "Inventarios";

        public InventarioController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Inventario>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Inventario> inventarios))
            {
                inventarios = new List<Inventario>();
                _cache.Set(CacheKey, inventarios);
            }
            return Ok(inventarios);
        }

        [HttpGet("{id}")]
        public ActionResult<Inventario> Get(int id)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound();
            return Ok(inventario);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Inventario inventario)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            inventario.Id = inventarios.Count > 0 ? inventarios.Max(i => i.Id) + 1 : 1;
            inventarios.Add(inventario);
            _cache.Set(CacheKey, inventarios);
            return CreatedAtAction(nameof(Get), new { id = inventario.Id }, inventario);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Inventario updatedInventario)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound();
            inventario.CantidadEnExistencia = updatedInventario.CantidadEnExistencia;
            inventario.BodegaId = updatedInventario.BodegaId;
            inventario.FechaIngreso = updatedInventario.FechaIngreso;
            inventario.FechaVencimiento = updatedInventario.FechaVencimiento;
            inventario.TipoLicor = updatedInventario.TipoLicor;
            _cache.Set(CacheKey, inventarios);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound();
            inventarios.Remove(inventario);
            _cache.Set(CacheKey, inventarios);
            return NoContent();
        }
    }
}
