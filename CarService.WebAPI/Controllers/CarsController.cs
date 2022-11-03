using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarService.WebAPI.Data;
using CarService.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CarService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarsService _carsService;
        private const string cacheKey = "carsList";
        private IMemoryCache _cache;

        public CarsController(ICarsService carsService, IMemoryCache cache)
        {
            _carsService = carsService;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] Filters filters)
        {
            IEnumerable<Car> cars;

            if (_cache.TryGetValue(cacheKey, out cars))
            {
                
            }
            else
            {
                cars = await _carsService.Get(null, filters);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, cars, cacheEntryOptions);
            }
            return Ok(cars);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car)
        {
            await _carsService.Add(car);
            _cache.Remove(cacheKey);
            return Ok(car);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _carsService.Delete(user);
            _cache.Remove(cacheKey);

            return NoContent();
        }
    }
}
