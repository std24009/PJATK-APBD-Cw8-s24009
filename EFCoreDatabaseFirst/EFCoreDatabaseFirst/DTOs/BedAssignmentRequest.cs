namespace EFCoreDatabaseFirst.DTOs;

public record BedAssignmentRequest(
    DateTime From,
    DateTime? To,
    string BedType,
    string Ward
    );