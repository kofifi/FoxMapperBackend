using FoxMapperBackend.Data;
using FoxMapperBackend.Models;
using FoxMapperBackend.Models.DTOs;
using FoxMapperBackend.Models.Requests;
using FoxMapperBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoxMapperBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouriersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICourierLocationStore _locationStore;

    public CouriersController(AppDbContext db, ICourierLocationStore locationStore)
    {
        _db = db;
        _locationStore = locationStore;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Courier>>> GetAll()
    {
        var couriers = await _db.Couriers.AsNoTracking().ToListAsync();
        return Ok(couriers);
    }

    [HttpPost]
    public async Task<ActionResult<Courier>> Create([FromBody] Courier courier)
    {
        courier.CreatedAtUtc = DateTime.UtcNow;
        courier.LastUpdatedUtc = DateTime.UtcNow;

        _db.Couriers.Add(courier);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = courier.Id }, courier);
    }

    // PUT: api/couriers/{id}/location
    [HttpPut("{id:int}/location")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateCourierLocationRequest request)
    {
        var courierExists = await _db.Couriers.AsNoTracking().AnyAsync(c => c.Id == id);
        if (!courierExists)
            return NotFound($"Courier with id {id} not found.");

        var location = new CourierLocationDto
        {
            Lat = request.Lat,
            Lng = request.Lng,
            AccuracyM = request.AccuracyM,
            HeadingDeg = request.HeadingDeg,
            SpeedMps = request.SpeedMps,
            ObservedAtUtc = request.ObservedAtUtc ?? DateTime.UtcNow
        };

        _locationStore.Set(id, location);
        return NoContent();
    }
}
