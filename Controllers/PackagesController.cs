using FoxMapperBackend.Data;
using FoxMapperBackend.Models;
using FoxMapperBackend.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoxMapperBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PackagesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/packages/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var package = await _db.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        return Ok(new
        {
            package.Id,
            package.DeliveryRunId,
            package.ExternalCode,
            package.RecipientName,
            package.Lat,
            package.Lng,
            Status = package.Status.ToString(),
            package.CreatedAtUtc,
            package.LastUpdatedUtc
        });
    }

    // PATCH: api/packages/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePackageStatusRequest request)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == id);
        if (package == null)
            return NotFound($"Package with id {id} not found.");

        // Proste nadpisanie statusu – offline/konflikty możesz ogarnąć później
        package.Status = request.Status;

        if (request.Status == PackageStatus.Delivered && package.DeliveredAtUtc == null)
        {
            package.DeliveredAtUtc = DateTime.UtcNow;
        }

        package.LastUpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}