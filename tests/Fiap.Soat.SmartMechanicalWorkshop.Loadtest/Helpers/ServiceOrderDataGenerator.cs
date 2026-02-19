using Bogus;
namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
public class ServiceOrderDataGenerator
{
    private readonly Faker _faker;
    public ServiceOrderDataGenerator()
    {
        _faker = new Faker("pt_BR");
    }
    public string GenerateTitle()
    {
        var titles = new[] { "Revisão periódica", "Troca de óleo", "Manutenção preventiva", "Alinhamento", "Troca de freio" };
        return _faker.PickRandom(titles);
    }
    public string GenerateDescription()
    {
        var descriptions = new[] { "Realizar manutenção completa", "Verificar itens de segurança", "Cliente relatou ruídos", "Manutenção preventiva" };
        return _faker.PickRandom(descriptions);
    }
}
