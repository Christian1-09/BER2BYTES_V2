using System.Text.Json;

namespace Ber2Bytes.Infrastructure.Storage;

/// <summary>Per-computer preferences saved in prefs.json (e.g. "Remember me").</summary>
public class JsonUserPreferences : IUserPreferences
{
    private readonly IAppLogger _log;

    public string? RememberedUsername { get; set; }
    public bool SidebarCollapsed { get; set; }

    public JsonUserPreferences(IAppLogger log)
    {
        _log = log;
        try
        {
            if (File.Exists(AppPaths.PrefsFile))
            {
                var saved = JsonSerializer.Deserialize<PrefsData>(File.ReadAllText(AppPaths.PrefsFile));
                RememberedUsername = saved?.RememberedUsername;
                SidebarCollapsed = saved?.SidebarCollapsed ?? false;
            }
        }
        catch (Exception ex) { _log.Error("Could not read prefs", ex); }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(AppPaths.PrefsFile, JsonSerializer.Serialize(new PrefsData(RememberedUsername, SidebarCollapsed)));
        }
        catch (Exception ex) { _log.Error("Could not save prefs", ex); }
    }

    private record PrefsData(string? RememberedUsername, bool SidebarCollapsed);
}
