namespace Application.Common.Interfaces;

public interface IJwtService
{       
    string SecretKey { get; set; }
    string Issuer { get; set; }
    string Audience { get; set; }   
    double DurationInDays { get; set; }
    
}
