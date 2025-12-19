using Application.Common.Interfaces;

namespace Application.Common.Configuration;

public class JwtOptions : IJwtService
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public double DurationInDays { get; set; }
}
