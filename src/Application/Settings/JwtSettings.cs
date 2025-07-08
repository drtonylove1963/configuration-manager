namespace Application.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool RequireExpirationTime { get; set; } = true;
    public int ClockSkewMinutes { get; set; } = 5;
    
    public TimeSpan AccessTokenExpiry => TimeSpan.FromMinutes(AccessTokenExpiryMinutes);
    public TimeSpan RefreshTokenExpiry => TimeSpan.FromDays(RefreshTokenExpiryDays);
    public TimeSpan ClockSkew => TimeSpan.FromMinutes(ClockSkewMinutes);
}
