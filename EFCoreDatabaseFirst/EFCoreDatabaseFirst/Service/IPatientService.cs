using EFCoreDatabaseFirst.DTOs;

namespace EFCoreDatabaseFirst.Service;

public interface IPatientService
{
    Task<IEnumerable<PatientDetailsResponse>> GetAllPatientsAsync(CancellationToken cancellationToken, string? search);
    Task AddBedAssignmentAsync(string pesel, BedAssignmentRequest request, CancellationToken cancellationToken);
}