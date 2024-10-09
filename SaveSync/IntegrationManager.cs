using System.Reflection;
using SaveSync.Configs;
using SaveSync.Integrations;

namespace SaveSync;

public class IntegrationManager
{
    public static SettingsConfig SettingsConfig = new();
    private readonly List<BaseIntegration?> _integrations;

    public IntegrationManager()
    {
        _integrations = LoadIntegrations();
        LoadConfiguration();
        SettingsConfig.Save();
    }

    private BaseIntegration? SelectIntegration()
    {
        BaseIntegration SelectedIntegration = null;
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
            SettingsConfig.SelectedIntegrationName = SelectedIntegration.GetType().Name;

            var task = SelectedIntegration.DisplayUiOptions();
            task.Wait();
        }
        else
        {
            Console.WriteLine("Invalid selection. Please try again.");
        }

        return SelectedIntegration;
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
        SettingsConfig = SettingsConfig.LoadJson() ?? new SettingsConfig();
        if (SettingsConfig.TryLoadExisting(_integrations) == false)
        {
            SettingsConfig.SelectedIntegration = SelectIntegration();
        }
    }
}