namespace EFCoreDatabaseFirst.DTOs;

public record RoomResponse(
    string Id,
    bool HasTv,
    WardResponse Ward
    );