using System.Text.Json;

namespace Duplicator
{
  public class AppSettings
  {
    public string? FolderPath { get; set; }
  }

  public class SettingsManager
  {
    private readonly string _configDir;
    private readonly string _configPath;

    public SettingsManager()
    {
      var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      _configDir = Path.Combine(appData, "Duplicator");
      _configPath = Path.Combine(_configDir, "config.json");
    }

    public AppSettings Load()
    {
      try
      {
        if (File.Exists(_configPath))
        {
          var json = File.ReadAllText(_configPath);
          var settings = JsonSerializer.Deserialize<AppSettings>(json);
          return settings ?? new AppSettings();
        }
      }
      catch
      {
        // If reading fails, fall back to defaults
      }
      return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
      Directory.CreateDirectory(_configDir);
      var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      File.WriteAllText(_configPath, json);
    }
  }
}
