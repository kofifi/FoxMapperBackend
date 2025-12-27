using FoxMapperBackend.Data;
using FoxMapperBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoxMapperBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepotsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DepotsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Depot>>> GetAll()
    {
        var depots = await _db.Depots.AsNoTracking().ToListAsync();
        return Ok(depots);
    }

    [HttpPost]
    public async Task<ActionResult<Depot>> Create([FromBody] Depot depot)
    {
        depot.CreatedAtUtc = DateTime.UtcNow;
        depot.LastUpdatedUtc = DateTime.UtcNow;

        _db.Depots.Add(depot);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = depot.Id }, depot);
    }
}