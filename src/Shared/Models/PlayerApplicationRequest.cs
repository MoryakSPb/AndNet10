using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models;

public record PlayerApplicationRequest
{
    [MinLength(1, ErrorMessage = "Никнейм не может быть пустым")]
    [MaxLength(24, ErrorMessage = "Никнейм не может быть более 24 символов")]
    [Required(ErrorMessage = "Необходимо указать никнейм")]
    public string Nickname { get; set; } = string.Empty;

    [MaxLength(64, ErrorMessage = "Имя не может быть более 64 символов")]
    public string? RealName { get; set; }

    [MaxLength(96, ErrorMessage = "Слишком длинное имя пользователя Discord")]
    [Required(ErrorMessage = "Необходимо указать имя пользователя Discord")]
    [MinLength(1, ErrorMessage = "Необходимо указать имя пользователя Discord")]
    public string DiscordUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Необходимо указать имя пользователя Discord")]
    [Range(typeof(ulong), "1", "18446744073709551615", ConvertValueInInvariantCulture = true,
        ParseLimitsInInvariantCulture = true, ErrorMessage = "Необходимо указать профиль Discord")]
    public ulong DiscordId { get; set; }

    [MaxLength(512)]
    [Required(ErrorMessage = "Необходимо указать ссылку на аккаунт Steam")]
    [MinLength(1, ErrorMessage = "Необходимо указать ссылку на аккаунт Steam")]
    public string SteamLink { get; set; } = string.Empty;

    [Required(ErrorMessage = "Необходимо указать ссылку на аккаунт Steam")]
    [Range(typeof(ulong), "1", "18446744073709551615", ConvertValueInInvariantCulture = true,
        ParseLimitsInInvariantCulture = true, ErrorMessage = "Необходимо указать профиль Steam")]
    public ulong SteamId { get; set; }

    [Range(0, ushort.MaxValue,
        ErrorMessage = "Количество часов в игре должно быть указано в диапазоне между 0 и 65535")]
    public int? Hours { get; set; }

    [Range(0, byte.MaxValue, ErrorMessage = "Возраст должен быть указан в диапазоне между 0 и 255")]
    public int? Age { get; set; }

    [MaxLength(512)]
    public string? TimeZoneId
    {
        get => TimeZone?.Id;
        set => TimeZone = value is null ? null : TimeZoneInfo.FindSystemTimeZoneById(value);
    }

    [JsonIgnore]
    public TimeZoneInfo? TimeZone { get; set; } = TimeZoneInfo.Local;

    [MaxLength(512, ErrorMessage = "Рекомендация должна быть не более 512 символов")]
    public string Recommendation { get; set; } = string.Empty;

    [MaxLength(16384, ErrorMessage = "Роп. описание должно быть не более 16384 символов")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Необходимо подтвердить согласие с уставом клана")]
    public bool RulesAgreed { get; set; }
}