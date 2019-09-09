using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistributedCacheDemo.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedCacheDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public ValuesController(IDistributedCache cache)
        {
            this._cache = cache;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync()
        {
            Func<IEnumerable<string>> values = () => new string[] { "value1", "value2" };
            return Ok(await _cache.FromAsync(Cache.Tag.Name.Values, null, values, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }));
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetAsync(int id)
        {
            Func<string> values = () => "value";
            return Ok(await _cache.FromAsync(Cache.Tag.Name.Values, new { id }, values, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }));
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] string value)
        {
            await _cache.RemoveAsync(Cache.Tag.Name.Values);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] string value)
        {
            // remove specific values object from cache using id
            await _cache.RemoveAsync(Cache.Tag.Name.Values, new { id });

            // OR remove all values objects from cache
            await _cache.RemoveAsync(Cache.Tag.Name.Values);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            // remove specific values object from cache using id
            await _cache.RemoveAsync(Cache.Tag.Name.Values, new { id });

            // OR remove all values objects from cache
            await _cache.RemoveAsync(Cache.Tag.Name.Values);
        }
    }
}
