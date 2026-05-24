namespace EFCoreDatabaseFirst.DTOs;

public record PatientDetailsResponse(
    string Pesel,
    string FirstName,
    string LastName,
    int Age,
    bool Sex,
    List<AdmissionResponse> Admissions,
    List<BedAssignmentResponse> BedAssignments
    );