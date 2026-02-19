using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Config;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Scenarios;
using Microsoft.Extensions.Configuration;
using NBomber.CSharp;

namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest;

/// <summary>
/// NBomber Load Test for Smart Mechanical Workshop API
///
/// Architecture: This load test is designed to run against a free-tier AWS EKS cluster
/// with minimal resource consumption while generating realistic data for NewRelic monitoring.
///
/// Test Strategy:
/// 1. Light load to avoid excessive costs on free tier
/// 2. Focus on Service Order lifecycle (main business flow)
/// 3. Validate read operations across different endpoints
/// 4. Generate sufficient data for observability in NewRelic
///
/// References:
/// - NBomber Documentation: https://nbomber.com/docs/overview
/// - Load Testing Best Practices: https://learn.microsoft.com/azure/architecture/best-practices/load-testing
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        PrintBanner();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var config = configuration.GetSection("LoadTest").Get<LoadTestConfig>()
            ?? throw new Exception("Failed to load configuration");

        Console.WriteLine("📋 Configuration:");
        Console.WriteLine($"   API: {config.ApiBaseUrl}");
        Console.WriteLine($"   Virtual Users: {config.VirtualUsers}");
        Console.WriteLine($"   Duration: {config.TestDuration}");
        Console.WriteLine($"   Ramp-up: {config.RampUpSeconds}s");
        Console.WriteLine();

        try
        {
            // Step 1: Authenticate
            Console.WriteLine("🔐 Authenticating...");
            var apiClient = new ApiClient(config.ApiBaseUrl);
            var authToken = await apiClient.AuthenticateAsync(config.LoginEmail, config.LoginPassword);
            Console.WriteLine("✅ Authentication successful");
            Console.WriteLine();

            // Step 2: Load existing data from database
            var dataProvider = new DataProvider(apiClient.GetHttpClient());
            await dataProvider.LoadDataAsync();
            Console.WriteLine();

            // Validate we have enough data
            if (dataProvider.GetClientCount() < 5 || dataProvider.GetVehicleCount() < 5)
            {
                Console.WriteLine("⚠️  WARNING: Limited data in database. Consider adding more clients and vehicles.");
                Console.WriteLine("   The test will continue but may have reduced variety.");
                Console.WriteLine();
            }

            // Step 3: Create test scenarios
            Console.WriteLine("🎬 Creating test scenarios...");

            var lifecycleScenario = new ServiceOrderLifecycleScenario(dataProvider, authToken, config.ApiBaseUrl);
            var readScenario = new ReadOperationsScenario(dataProvider, authToken, config.ApiBaseUrl);

            var mainScenario = lifecycleScenario.CreateScenario(
                (int)config.TestDuration.TotalSeconds,
                config.VirtualUsers,
                config.RampUpSeconds
            );

            var readsScenario = readScenario.CreateScenario(
                (int)config.TestDuration.TotalSeconds,
                config.VirtualUsers,
                config.RampUpSeconds
            );

            Console.WriteLine("✅ Scenarios created");
            Console.WriteLine();

            // Step 4: Run the test
            Console.WriteLine("🚀 Starting Load Test...");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine();

            NBomberRunner
                .RegisterScenarios(mainScenario, readsScenario)
                .WithReportFileName("load_test_report")
                .WithReportFolder("./reports")
                .Run();

            Console.WriteLine();
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("✅ Load Test Completed!");
            Console.WriteLine("📁 Reports generated in ./reports folder");
            Console.WriteLine("📈 Check NewRelic dashboard for detailed metrics and traces");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ Load Test Failed!");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    private static void PrintBanner()
    {
        Console.Clear();
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║        Smart Mechanical Workshop - Load Test Suite           ║
║                      Powered by NBomber                       ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
");
        Console.WriteLine("🎯 Objective: Generate realistic load and data for NewRelic");
        Console.WriteLine("🏗️  Architecture: Light load optimized for AWS Free Tier");
        Console.WriteLine();
    }
}

