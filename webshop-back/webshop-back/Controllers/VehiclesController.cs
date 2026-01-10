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

        // GET: api/vehicles
        [HttpGet]
        public IActionResult Get()
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            // Require merchant context for listing (multi-tenant)
            if (!merchantId.HasValue)
            {
                return BadRequest("Merchant context required.");
            }

            var vehicles = _repo.GetVehiclesForMerchant(merchantId.Value);
            return Ok(vehicles);
        }

        // GET: api/vehicles/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            if (!merchantId.HasValue)
            {
                return BadRequest("Merchant context required.");
            }

            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            if (vehicle.MerchantId != merchantId.Value)
                return NotFound(); // hide existence of vehicles from other merchants

            return Ok(vehicle);
        }

        // GET: api/vehicles/{id}/image
        [HttpGet("{id}/image")]
        public IActionResult GetImage(int id)
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            if (!merchantId.HasValue)
            {
                return BadRequest("Merchant context required.");
            }

            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            if (vehicle.MerchantId != merchantId.Value)
                return NotFound();

            var img = vehicle.Image;
            if (img == null || img.Length == 0) return NotFound();

            return File(img, "image/jpeg");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromForm] VehicleDto dto, IFormFile? image)
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            if (!merchantId.HasValue)
            {
                return Forbid("Merchant context required to create a vehicle.");
            }

            var vehicle = new Vehicle
            {
                Make = dto.Make,
                Model = dto.Model,
                Description = dto.Description,
                Price = dto.Price,
                MerchantId = merchantId.Value
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] VehicleDto dto, IFormFile? image)
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            if (!merchantId.HasValue)
                return Forbid("Merchant context required.");

            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            if (vehicle.MerchantId != merchantId.Value)
                return NotFound();

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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var merchant = HttpContext.Items["Merchant"] as Merchant;
            var merchantId = merchant?.MerchantId;

            if (!merchantId.HasValue)
                return Forbid("Merchant context required.");

            var vehicle = _repo.GetVehicle(id);
            if (vehicle == null) return NotFound();

            if (vehicle.MerchantId != merchantId.Value)
                return NotFound();

            _repo.DeleteVehicle(id);
            return NoContent();
        }
    }
}
