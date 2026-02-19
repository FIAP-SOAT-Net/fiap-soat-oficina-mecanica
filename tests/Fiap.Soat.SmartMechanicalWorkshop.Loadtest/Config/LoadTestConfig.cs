namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Config;
public class LoadTestConfig
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string LoginEmail { get; set; } = string.Empty;
    public string LoginPassword { get; set; } = string.Empty;
    public TimeSpan TestDuration { get; set; }
    public int VirtualUsers { get; set; }
    public int RampUpSeconds { get; set; }
    public int ServiceOrdersToCreate { get; set; }
}
