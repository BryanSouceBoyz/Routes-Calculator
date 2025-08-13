using Microsoft.AspNetCore.Mvc;
using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Domain.Entities;

namespace RoutesCalculator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostsController : ControllerBase
    {
        private readonly ICostService _costService;
        public CostsController(ICostService costService) => _costService = costService;

        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] TripInput input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var options = _costService.CalculateAll(input).ToList();
            var best = options.First();
            return Ok(new { best, options });
        }
    }
}
