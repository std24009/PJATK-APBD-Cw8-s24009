using EFCoreDatabaseFirst.DTOs;
using EFCoreDatabaseFirst.Exceptions;
using EFCoreDatabaseFirst.Service;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreDatabaseFirst.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(IPatientService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPatients(CancellationToken cancellationToken,
        [FromQuery] string? search = null)
    {
        var result = await service.GetAllPatientsAsync(cancellationToken, search);
        /*try
        {
            return Ok(await service.GetAllPatientsAsync(cancellationToken, search));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }*/
        if (!result.Any())
        {
            return NotFound($"Patient with search {search} not found");
        }

        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")] 
    public async Task<IActionResult> CreateBedAssignment(string pesel, [FromBody] BedAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.AddBedAssignmentAsync(pesel, request, cancellationToken);
            return Created(string.Empty, new { Message = "Bed assignment created successfully." });

        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { Message = e.Message });
        }
        catch(NotFoundException e)
        {
            return NotFound(new { Message = e.Message });
        }
    }
}