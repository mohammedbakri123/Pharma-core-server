namespace PharmaCore.Application.System.Dtos;

public sealed record EnumValueDto(string Name, short Value);

public sealed record EnumsDto(IReadOnlyDictionary<string, IReadOnlyList<EnumValueDto>> Enums);
