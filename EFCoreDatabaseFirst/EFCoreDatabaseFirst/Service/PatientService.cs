using EFCoreDatabaseFirst.DTOs;
using EFCoreDatabaseFirst.Exceptions;
using EFCoreDatabaseFirst.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDatabaseFirst.Service;

public class PatientService(MasterContext ctx) : IPatientService
{
    public async Task<IEnumerable<PatientDetailsResponse>> GetAllPatientsAsync(CancellationToken cancellationToken, string? search)
    {
        var query = ctx.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(p => p.LastName.Contains(searchTerm) || p.FirstName.Contains(searchTerm));
        }

        return await query
                   .Select(p => new PatientDetailsResponse(
                       p.Pesel,
                       p.FirstName,
                       p.LastName,
                       p.Age,
                       p.Sex,
                       p.Admissions.Select(ad => new AdmissionResponse(
                           ad.Id,
                           ad.AdmissionDate,
                           ad.DischargeDate,
                           new WardResponse(
                               ad.Ward.Id,
                               ad.Ward.Name,
                               ad.Ward.Description
                           )
                       )).ToList(),
                       p.BedAssignments.Select(ba => new BedAssignmentResponse(
                               ba.Id,
                               ba.From,
                               ba.To,
                               new BedResponse(
                                   ba.Bed.Id,
                                   new BedTypeResponse(
                                       ba.Bed.BedType.Id,
                                       ba.Bed.BedType.Name,
                                       ba.Bed.BedType.Description),
                                   new RoomResponse(
                                       ba.Bed.Room.Id,
                                       ba.Bed.Room.HasTv,
                                       new WardResponse(
                                           ba.Bed.Room.Ward.Id,
                                           ba.Bed.Room.Ward.Name,
                                           ba.Bed.Room.Ward.Description)
                                   )
                               )
                           )
                       ).ToList()
                   )).ToListAsync(cancellationToken)
               ?? throw new NotFoundException($"Patient with search {search} not found");
    }
}