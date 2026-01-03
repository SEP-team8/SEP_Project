using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webshop_back.DTOs;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

namespace webshop_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IRepository _repo;

        public VehiclesController(IRepository repo)
        {
            _repo = repo;
        }

        // GET /api/vehicles
        [HttpGet]
        public IActionResult Get()
        {
            var vehicles = _repo.GetVehicles();
            return Ok(vehicles);
        }

        // GET /api/vehicles/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        // GET /api/vehicles/{id}/image
        [HttpGet("{id}/image")]
        public IActionResult GetImage(int id)
        {
            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null || vehicle.Image == null || vehicle.Image.Length == 0)
                return NotFound();

            return File(vehicle.Image, "image/jpeg");
        }

        // CREATE Vehicle - Admin only
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromForm] VehicleDto dto, IFormFile? image)
        {
            var vehicle = new Vehicle
            {
                Make = dto.Make,
                Model = dto.Model,
                Description = dto.Description,
                Price = dto.Price
            };

            if (image != null)
            {
                using var ms = new MemoryStream();
                image.CopyTo(ms);
                vehicle.Image = ms.ToArray();
            }

            _repo.AddVehicle(vehicle);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        // UPDATE Vehicle - Admin only
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] VehicleDto dto, IFormFile? image)
        {
            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Description = dto.Description;
            vehicle.Price = dto.Price;

            if (image != null)
            {
                using var ms = new MemoryStream();
                image.CopyTo(ms);
                vehicle.Image = ms.ToArray();
            }

            _repo.UpdateVehicle(vehicle);
            return Ok(vehicle);
        }

        // DELETE Vehicle - Admin only
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            _repo.DeleteVehicle(id);
            return NoContent();
        }
    }
}
