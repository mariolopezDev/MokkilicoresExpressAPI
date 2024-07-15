using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "Direcciones";

        public DireccionController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Direccion>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Direccion>? direcciones))
            {
                direcciones = new List<Direccion>();
                _cache.Set(CacheKey, direcciones);
            }
            return Ok(direcciones);
        }

        [HttpGet("{id}")]
        public ActionResult<Direccion> Get(int id)
        {
            var direcciones = _cache.Get<List<Direccion>>(CacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound();
            return Ok(direccion);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Direccion direccion)
        {
            var direcciones = _cache.Get<List<Direccion>>(CacheKey);
            direccion.Id = direcciones.Count > 0 ? direcciones.Max(d => d.Id) + 1 : 1;
            direcciones.Add(direccion);
            _cache.Set(CacheKey, direcciones);
            return CreatedAtAction(nameof(Get), new { id = direccion.Id }, direccion);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Direccion updatedDireccion)
        {
            var direcciones = _cache.Get<List<Direccion>>(CacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound();
            direccion.Provincia = updatedDireccion.Provincia;
            direccion.Canton = updatedDireccion.Canton;
            direccion.Distrito = updatedDireccion.Distrito;
            direccion.PuntoEnWaze = updatedDireccion.PuntoEnWaze;
            direccion.EsCondominio = updatedDireccion.EsCondominio;
            _cache.Set(CacheKey, direcciones);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var direcciones = _cache.Get<List<Direccion>>(CacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound();
            direcciones.Remove(direccion);
            _cache.Set(CacheKey, direcciones);
            return NoContent();
        }
    }
}
