using Microsoft.AspNetCore.Mvc;
using webshop_back.Service;

namespace webshop_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IRepository _repo;
        public VehiclesController(IRepository repo) { _repo = repo; }

        [HttpGet]
        public IActionResult Get() => Ok(_repo.GetVehicles());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var v = _repo.GetVehicle(id);
            if (v == null) return NotFound();
            return Ok(v);
        }
    }
}
