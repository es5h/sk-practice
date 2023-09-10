using System.ComponentModel.DataAnnotations;

public sealed class AiServiceOptions
{
    [Required] public string Key { get; init; }
}