using FlowFi.Domain.Common;

namespace FlowFi.Domain.Entities;

public class Category : BaseEntity
{
    public Guid? UserId { get; private set; }   // null = system default
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public bool IsDefault { get; private set; }

    public User? User { get; private set; }

    private Category() { }

    public static Category CreateDefault(string name, string? icon, string? color) =>
        new() { Name = name, Icon = icon, Color = color, IsDefault = true };

    public static Category CreateUserDefined(Guid userId, string name, string? icon, string? color) =>
        new() { UserId = userId, Name = name, Icon = icon, Color = color, IsDefault = false };
}
