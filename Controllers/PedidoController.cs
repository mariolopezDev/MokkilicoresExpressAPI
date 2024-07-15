using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "Pedidos";

        public PedidoController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Pedido>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Pedido> pedidos))
            {
                pedidos = new List<Pedido>();
                _cache.Set(CacheKey, pedidos);
            }
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public ActionResult<Pedido> Get(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
                return NotFound();
            return Ok(pedido);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Pedido pedido)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            pedido.Id = pedidos.Count > 0 ? pedidos.Max(p => p.Id) + 1 : 1;
            pedido.CostoTotal = pedido.CostoSinIVA * 1.13m * pedido.Cantidad;
            pedidos.Add(pedido);
            _cache.Set(CacheKey, pedidos);
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Pedido updatedPedido)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
                return NotFound();
            pedido.ProductoId = updatedPedido.ProductoId;
            pedido.Cantidad = updatedPedido.Cantidad;
            pedido.CostoSinIVA = updatedPedido.CostoSinIVA;
            pedido.CostoTotal = updatedPedido.CostoSinIVA * 1.13m * updatedPedido.Cantidad;
            pedido.Estado = updatedPedido.Estado;
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
                return NotFound();
            pedidos.Remove(pedido);
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }
    }
}
