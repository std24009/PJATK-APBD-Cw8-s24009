using EFCoreDatabaseFirst.DTOs;
using EFCoreDatabaseFirst.Exceptions;
using EFCoreDatabaseFirst.Infrastructure;
using EFCoreDatabaseFirst.Models;
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

    public async Task AddBedAssignmentAsync(string pesel, BedAssignmentRequest request, CancellationToken cancellationToken)
    {
        var patientExist = await ctx.Patients.AnyAsync(p => p.Pesel == pesel, cancellationToken);
        if (!patientExist) throw new KeyNotFoundException($"Patient with PESEL {pesel} does not exist.");

        var wardExist = await ctx.Wards.FirstOrDefaultAsync(w => w.Name == request.Ward, cancellationToken);
        if (wardExist == null) throw new KeyNotFoundException($"Ward '{request.Ward}' does not exist.");
        
        var bedTypeExist = await ctx.BedTypes.FirstOrDefaultAsync(b => b.Name == request.BedType, cancellationToken);
        if (bedTypeExist == null) throw new KeyNotFoundException($"Bed type '{request.BedType}' does not exist.");

        if (request.To.HasValue && request.From >= request.To.Value)
        {
            throw new ArgumentException($"The start date {request.From} cannot be later than end date {request.To}");
        }

        var requestedFrom = request.From;
        var requestedTo = request.To ?? DateTime.MaxValue;

        var availableBed = await ctx.Beds
            .Where(b => b.Room.WardId == wardExist.Id && b.BedTypeId == bedTypeExist.Id)
            .Where(b => !ctx.BedAssignments.Any(ba =>
                ba.BedId == b.Id &&
                ((ba.To != null && requestedFrom < ba.To && requestedTo > ba.From) ||
                (ba.To == null && requestedTo > ba.From))
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (availableBed == null)
            throw new InvalidOperationException(
                $"No available bed of type '{request.BedType}' found in ward '{request.Ward}' for the requested period.");

        var newAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = request.From,
            To = request.To
        };

        ctx.BedAssignments.Add(newAssignment);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}