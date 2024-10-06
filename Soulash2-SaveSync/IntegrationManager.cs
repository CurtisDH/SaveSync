using System.Reflection;
using System.Text.Json;
using Soulash2_SaveSync.Configs;
using Soulash2_SaveSync.Integrations;
namespace Soulash2_SaveSync;

public class IntegrationManager
{
    private readonly string _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "integration_config.json");
    private SettingsConfig IntegrationSettings = new(); 
    public BaseIntegration? SelectedIntegration { get; private set; }
    private readonly List<BaseIntegration?> _integrations;

    public IntegrationManager()
    {
        _integrations = LoadIntegrations();
        LoadConfiguration();
        SaveConfiguration();
    }

    private void SaveConfiguration()
    {
        var s = JsonSerializer.Serialize(IntegrationSettings);
        File.WriteAllText(_configFilePath,s);
    }

    ~IntegrationManager()
    {
        SaveConfiguration();
    }

    private void SelectIntegration()
    {
        Console.WriteLine("== Select Save Integration ==");
        for (var i = 0; i < _integrations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_integrations[i]?.GetType().Name}");
        }

        if (int.TryParse(Console.ReadLine(), out int selectedOption) && selectedOption > 0 &&
            selectedOption <= _integrations.Count)
        {
            SelectedIntegration = _integrations[selectedOption - 1];
            Console.WriteLine($"Integration set to: {SelectedIntegration?.GetType().Name}");
            IntegrationSettings.SelectedIntegrationName = SelectedIntegration.GetType().Name;
            SelectedIntegration?.DisplayUiOptions();
        }
        else
        {
            Console.WriteLine("Invalid selection. Please try again.");
        }
    }
    
    private List<BaseIntegration?> LoadIntegrations()
    {
        var baseType = typeof(BaseIntegration);
        var integrationTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract);

        var loadedIntegrations = new List<BaseIntegration?>();
        foreach (var type in integrationTypes)
        {
            if (Activator.CreateInstance(type) is BaseIntegration integrationInstance)
            {
                loadedIntegrations.Add(integrationInstance);
            }
        }

        return loadedIntegrations;
    }

    private void LoadConfiguration()
    {
        if (File.Exists(_configFilePath))
        {
            try
            {
                var config = JsonSerializer.Deserialize<SettingsConfig>(File.ReadAllText(_configFilePath));
                if (config == null) return;
                
                SelectedIntegration = _integrations.FirstOrDefault(i => i?.GetType().Name == config.SelectedIntegrationName);
                if (SelectedIntegration != null)
                {
                    Console.WriteLine($"Loaded existing selected integration: {SelectedIntegration.GetType().Name}");
                    IntegrationSettings.SelectedIntegrationName = SelectedIntegration.GetType().Name;
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
            }

        }
        SelectIntegration();
    }
}

