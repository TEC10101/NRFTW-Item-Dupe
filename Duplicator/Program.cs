using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Duplicator
{
  public class Program
  {
    private const string CharacterGuid = "cc49dd5a-9557-4ce5-9293-7ca807696018";
    private const string RealmGuid = "fbe0e823-0478-4c99-a584-45e6b94c6a50";

    public static void Main(string[] args)
    {
      Console.OutputEncoding = Encoding.UTF8;

      var settingsManager = new SettingsManager();
      var settings = settingsManager.Load();

      while (true)
      {
        RenderMenu(settings);
        var keyInfo = Console.ReadKey(intercept: true);

        switch (keyInfo.Key)
        {
          case ConsoleKey.F1:
            ShowCharacterMenu(settings);
            break;



          case ConsoleKey.F4:
            WriteInfo("F4 pressed: placeholder command to be implemented.");
            // TODO: Implement command execution for F4
            Pause();
            break;

          case ConsoleKey.F5:
            ShowRealmMenu(settings);
            break;

          case ConsoleKey.F9:
            UpdateFolderPathScreen(settingsManager, settings);
            break;

          case ConsoleKey.Escape:
          case ConsoleKey.Q:
            WriteInfo("Exiting…");
            return;

          default:
            // Ignore other keys
            break;
        }
      }
    }

    private static void RenderMenu(AppSettings settings)
    {
      Console.Clear();
      DrawHeader("=== Duplicator ===", null);
      DrawLineAt(2, $"Current Folder: {settings.FolderPath ?? "<not set>"}");
      DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Character");
      DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Realm");
      DrawLineAt(GetRowForFKey(ConsoleKey.F9), "F9 - Update Folder Path");
      DrawLineAt(14, "ESC/Q - Quit");
    }

    private static void Pause()
    {
      Console.WriteLine();
      Console.Write("Press any key to return to menu…");
      Console.ReadKey(intercept: true);
    }

    private static void WriteSuccess(string message)
    {
      var prev = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(message);
      Console.ForegroundColor = prev;
    }

    private static void WriteWarn(string message)
    {
      var prev = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(message);
      Console.ForegroundColor = prev;
    }

    private static void WriteError(string message)
    {
      var prev = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(message);
      Console.ForegroundColor = prev;
    }

    private static void WriteInfo(string message)
    {
      Console.WriteLine(message);
    }

    private static void UpdateFolderPathScreen(SettingsManager settingsManager, AppSettings settings)
    {
      Console.Clear();
      Console.WriteLine("=== Select Folder Path ===");
      Console.WriteLine();

      var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      var basePath = Path.Combine(userProfile, "AppData", "LocalLow", "Moon Studios", "NoRestForTheWicked", "DataStore");

      Console.WriteLine($"Base: {basePath}");
      Console.WriteLine($"Current: {settings.FolderPath ?? "<not set>"}");
      Console.WriteLine();

      if (!Directory.Exists(basePath))
      {
        WriteError("Base DataStore folder not found.");
        Pause();
        return;
      }

      string[] candidates;
      try
      {
        candidates = Directory.GetDirectories(basePath)
          .Where(d =>
          {
            var name = Path.GetFileName(d);
            return name.All(char.IsDigit);
          })
          .OrderBy(d => Path.GetFileName(d))
          .Take(99)
          .ToArray();
      }
      catch (Exception ex)
      {
        WriteError($"Failed to read subfolders: {ex.Message}");
        Pause();
        return;
      }

      if (candidates.Length == 0)
      {
        WriteWarn("No 5-digit folders found in DataStore.");
        Pause();
        return;
      }

      for (int i = 0; i < candidates.Length; i++)
      {
        var name = Path.GetFileName(candidates[i]);
        Console.WriteLine($"{i + 1}. {name}");
      }

      Console.WriteLine();
      Console.Write("Enter number to select (or blank to cancel): ");
      var input = Console.ReadLine()?.Trim();
      if (string.IsNullOrWhiteSpace(input))
      {
        WriteInfo("No changes made.");
        Pause();
        return;
      }

      if (!int.TryParse(input, out var idx) || idx < 1 || idx > candidates.Length)
      {
        WriteError("Invalid selection.");
        Pause();
        return;
      }

      var selectedPath = candidates[idx - 1];
      settings.FolderPath = selectedPath;

      try
      {
        settingsManager.Save(settings);
        WriteSuccess($"Folder path set to {selectedPath}.");
      }
      catch (Exception ex)
      {
        WriteError($"Failed to save settings: {ex.Message}");
      }
      Pause();
    }

    private static void ShowCharacterMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Character ===", ConsoleColor.Yellow);
        DrawSelectedInfo("Character", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F2), "F2 - Backup Character Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F3), "F3 - Restore Character Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F4), "F4 - Select Character");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F1:
            return; // back to home
          case ConsoleKey.F2:
            ShowCharacterBackupMenu(settings);
            break;
          case ConsoleKey.F3:
            ShowCharacterRestoreMenu(settings);
            break;
          case ConsoleKey.F4:
            ShowCharacterSelectMenu(settings);
            break;
          default:
            break;
        }
      }
    }

    private static void ShowRealmMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Realm ===", ConsoleColor.Blue);
        DrawSelectedInfo("Realm", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F6), "F6 - Backup Realm Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F7), "F7 - Restore Realm Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F8), "F8 - Select Realm");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F5:
            return; // back to home
          case ConsoleKey.F6:
            ShowRealmBackupMenu(settings);
            break;
          case ConsoleKey.F7:
            ShowRealmRestoreMenu(settings);
            break;
          case ConsoleKey.F8:
            ShowRealmSelectMenu(settings);
            break;
          default:
            break;
        }
      }
    }

    private static void ShowSuccessAndWait(string message, int seconds = 3)
    {
      Console.WriteLine();
      Console.Write($"{message}");
      for (int i = 0; i < seconds; i++)
      {
        Thread.Sleep(1000);
        Console.Write('.');
      }
      Thread.Sleep(200); // small settle time
    }

    private static void ShowCharacterBackupMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Character Backup ===", ConsoleColor.Yellow);
        DrawSelectedInfo("Character", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F2), "F2 - Backup Character Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F1:
            return; // back to Character submenu
          case ConsoleKey.F2:
            {
              var guid = settings.CharacterGuid;
              if (string.IsNullOrWhiteSpace(guid))
              {
                WriteError("No Character selected. Press F4 to select.");
                Pause();
                break;
              }
              var archiveName = GetDatedArchiveName("Character", guid);
              var ok = RunWinRarAdd(settings, archiveName, guid);
              if (ok)
              {
                ShowSuccessAndWait("Returning to Character menu");
                return; // go back up one menu on success
              }
            }
            break;
          default:
            break;
        }
      }
    }

    private static void ShowRealmBackupMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Realm Backup ===", ConsoleColor.Blue);
        DrawSelectedInfo("Realm", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F6), "F6 - Backup Realm Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F5:
            return; // back to Realm submenu
          case ConsoleKey.F6:
            {
              var guid = settings.RealmGuid;
              if (string.IsNullOrWhiteSpace(guid))
              {
                WriteError("No Realm selected. Press F8 to select.");
                Pause();
                break;
              }
              var archiveName = GetDatedArchiveName("Realm", guid);
              var ok = RunWinRarAdd(settings, archiveName, guid);
              if (ok)
              {
                ShowSuccessAndWait("Returning to Realm menu");
                return; // go back up one menu on success
              }
            }
            break;
          default:
            break;
        }
      }
    }

    private static void ShowCharacterRestoreMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Character Restore ===", ConsoleColor.Yellow);
        DrawSelectedInfo("Character", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F3), "F3 - Restore Character Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F1:
            return; // back to Character submenu
          case ConsoleKey.F3:
            {
              var workingDir = settings.FolderPath ?? string.Empty;
              var guid = settings.CharacterGuid;
              if (string.IsNullOrWhiteSpace(guid))
              {
                WriteError("No Character selected. Press F4 to select.");
                Pause();
                break;
              }
              var latest = FindLatestArchive(workingDir, "Character", guid);
              if (latest is null)
              {
                WriteError("No Character backup found.");
                Pause();
                break;
              }

              if (!ConfirmRestoreIfOld(latest))
              {
                WriteWarn("Restore cancelled.");
                Pause();
                break;
              }

              var ok = RunWinRarExtract(settings, Path.GetFileName(latest));
              if (ok) ShowSuccessAndWait("Staying on Character Restore menu");
            }
            break;
          default:
            break;
        }
      }
    }

    private static void ShowRealmRestoreMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Realm Restore ===", ConsoleColor.Blue);
        DrawSelectedInfo("Realm", settings);
        DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F7), "F7 - Restore Realm Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F5:
            return; // back to Realm submenu
          case ConsoleKey.F7:
            {
              var workingDir = settings.FolderPath ?? string.Empty;
              var guid = settings.RealmGuid;
              if (string.IsNullOrWhiteSpace(guid))
              {
                WriteError("No Realm selected. Press F8 to select.");
                Pause();
                break;
              }
              var latest = FindLatestArchive(workingDir, "Realm", guid);
              if (latest is null)
              {
                WriteError("No Realm backup found.");
                Pause();
                break;
              }

              if (!ConfirmRestoreIfOld(latest))
              {
                WriteWarn("Restore cancelled.");
                Pause();
                break;
              }

              var ok = RunWinRarExtract(settings, Path.GetFileName(latest));
              if (ok) ShowSuccessAndWait("Staying on Realm Restore menu");
            }
            break;
          default:
            break;
        }
      }
    }

    private static void DrawSelectedInfo(string kind, AppSettings settings)
    {
      string? guid = kind == "Character" ? settings.CharacterGuid : settings.RealmGuid;
      if (string.IsNullOrWhiteSpace(guid))
      {
        DrawLineAt(2, $"Selected {kind}: <none>");
        return;
      }
      string? name = null;
      if (kind == "Character")
      {
        if (settings.CharacterNames != null && settings.CharacterNames.TryGetValue(guid, out var n)) name = n;
      }
      else
      {
        if (settings.RealmNames != null && settings.RealmNames.TryGetValue(guid, out var n)) name = n;
      }

      var prev = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      DrawLineAt(2, name is not null ? $"Selected {kind}: {name} ({guid})" : $"Selected {kind}: {guid}");
      Console.ForegroundColor = prev;
    }

    private static void DrawHeader(string title, ConsoleColor? color)
    {
      if (color.HasValue)
      {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color.Value;
        DrawLineAt(0, title);
        Console.ForegroundColor = prev;
      }
      else
      {
        DrawLineAt(0, title);
      }
    }

    private static void DrawLineAt(int row, string text)
    {
      try
      {
        Console.SetCursorPosition(0, row);
        Console.WriteLine(text);
      }
      catch
      {
        // Fallback if console is too small or does not support positioning
        Console.WriteLine(text);
      }
    }

    private static int GetRowForFKey(ConsoleKey key)
    {
      return key switch
      {
        ConsoleKey.F1 => 4,
        ConsoleKey.F2 => 5,
        ConsoleKey.F3 => 6,
        ConsoleKey.F4 => 7,
        ConsoleKey.F5 => 8,
        ConsoleKey.F6 => 9,
        ConsoleKey.F7 => 10,
        ConsoleKey.F8 => 11,
        ConsoleKey.F9 => 12,
        _ => 4
      };
    }



    private static bool RunWinRarAdd(AppSettings settings, string archiveName, string guid)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Press F1 to set it first.");
        Pause();
        return false;
      }

      var workingDir = settings.FolderPath!;

      var winRarCandidates = new[]
      {
        @"C:\\Program Files\\WinRAR\\WinRAR.exe",
        @"C:\\Program Files (x86)\\WinRAR\\WinRAR.exe"
      };

      string? winRarPath = null;
      foreach (var candidate in winRarCandidates)
      {
        if (File.Exists(candidate))
        {
          winRarPath = candidate;
          break;
        }
      }

      if (winRarPath is null)
      {
        WriteError("WinRAR.exe not found. Please install WinRAR or adjust the path.");
        Pause();
        return false;
      }

      var wildcard = $"*{guid}*";

      try
      {
        var psi = new ProcessStartInfo
        {
          FileName = winRarPath,
          Arguments = $"a \"{archiveName}\" \"{wildcard}\"",
          WorkingDirectory = workingDir,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
          WriteError("Failed to start WinRAR process.");
          Pause();
          return false;
        }

        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode == 0)
        {
          WriteSuccess($"Archive '{archiveName}' created successfully.");
          return true;
        }
        else
        {
          WriteWarn($"WinRAR exited with code {proc.ExitCode}.");
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
          Console.WriteLine(output);
        }
        if (!string.IsNullOrWhiteSpace(error))
        {
          WriteError(error);
        }
      }
      catch (Exception ex)
      {
        WriteError($"Error while running WinRAR: {ex.Message}");
      }

      Pause();
      return false;
    }



    private static bool RunWinRarExtract(AppSettings settings, string archiveName)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Press F1 to set it first.");
        Pause();
        return false;
      }

      var workingDir = settings.FolderPath!;
      var archivePath = Path.Combine(workingDir, archiveName);
      if (!File.Exists(archivePath))
      {
        WriteError($"{archiveName} not found in the selected folder.");
        Pause();
        return false;
      }

      var winRarCandidates = new[]
      {
        @"C:\\Program Files\\WinRAR\\WinRAR.exe",
        @"C:\\Program Files (x86)\\WinRAR\\WinRAR.exe"
      };

      string? winRarPath = null;
      foreach (var candidate in winRarCandidates)
      {
        if (File.Exists(candidate))
        {
          winRarPath = candidate;
          break;
        }
      }

      if (winRarPath is null)
      {
        WriteError("WinRAR.exe not found. Please install WinRAR or adjust the path.");
        Pause();
        return false;
      }

      try
      {
        var psi = new ProcessStartInfo
        {
          FileName = winRarPath,
          Arguments = $"x -o+ \"{archiveName}\"",
          WorkingDirectory = workingDir,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
          WriteError("Failed to start WinRAR process.");
          Pause();
          return false;
        }

        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode == 0)
        {
          try
          {
            var fi = new FileInfo(archivePath);
            var age = DateTime.UtcNow - fi.LastWriteTimeUtc;

            var lower = archiveName.ToLowerInvariant();
            var kind = lower.Contains("character") ? "character" :
                       lower.Contains("realm") ? "realm" : "data";

            string ago;
            if (age.TotalSeconds < 60)
            {
              var seconds = Math.Max(1, (int)Math.Round(age.TotalSeconds));
              ago = $"{seconds} seconds ago";
            }
            else
            {
              var minutes = Math.Max(1, (int)Math.Round(age.TotalMinutes));
              ago = $"{minutes} minutes ago";
            }

            WriteSuccess($"Restored {kind} to {ago}.");
          }
          catch
          {
            // Fallback to generic message if we can't compute age
            WriteSuccess("Extraction completed successfully.");
          }

          return true;
        }
        else
        {
          WriteWarn($"WinRAR exited with code {proc.ExitCode}.");
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
          Console.WriteLine(output);
        }
        if (!string.IsNullOrWhiteSpace(error))
        {
          WriteError(error);
        }
      }
      catch (Exception ex)
      {
        WriteError($"Error while running WinRAR: {ex.Message}");
      }

      Pause();
      return false;
    }

    private static string GetDatedArchiveName(string baseName, string guid)
    {
      var date = DateTime.Now.ToString("yyyy-MM-dd");
      return $"{date}_{baseName}_{guid}.rar";
    }

    private static string? FindLatestArchive(string workingDir, string baseName, string guid)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
        {
          return null;
        }
        var pattern = $"*_{baseName}_{guid}.rar";
        var files = Directory.GetFiles(workingDir, pattern);
        var latest = files
          .Select(f => new FileInfo(f))
          .OrderByDescending(fi => fi.LastWriteTimeUtc)
          .FirstOrDefault();
        return latest?.FullName;
      }
      catch
      {
        return null;
      }
    }

    private static void ShowCharacterSelectMenu(AppSettings settings)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Set it on home screen.");
        Pause();
        return;
      }

      var list = ScanGuids(settings.FolderPath!, "Character");
      Console.Clear();
      DrawHeader("=== Select Character ===", ConsoleColor.Yellow);
      if (list.Count == 0)
      {
        WriteWarn("No Character GUIDs found in filenames.");
        Pause();
        return;
      }

      for (int i = 0; i < list.Count; i++)
      {
        var guid = list[i];
        var name = settings.CharacterNames != null && settings.CharacterNames.TryGetValue(guid, out var n) ? n : null;
        Console.WriteLine($"{i + 1}. {(name is not null ? name + " - " : string.Empty)}{guid}");
      }
      Console.WriteLine();
      Console.Write("Enter number to select (or blank to cancel): ");
      var input = Console.ReadLine()?.Trim();
      if (string.IsNullOrWhiteSpace(input)) return;
      if (!int.TryParse(input, out var idx) || idx < 1 || idx > list.Count)
      {
        WriteError("Invalid selection.");
        Pause();
        return;
      }
      var selected = list[idx - 1];
      settings.CharacterGuid = selected;

      Console.Write("Name? (optional, leave blank to skip): ");
      var nameInput = Console.ReadLine()?.Trim();
      if (!string.IsNullOrWhiteSpace(nameInput))
      {
        settings.CharacterNames ??= new Dictionary<string, string>();
        settings.CharacterNames[selected] = nameInput;
      }

      try
      {
        new SettingsManager().Save(settings);
        WriteSuccess("Character selection saved.");
      }
      catch (Exception ex)
      {
        WriteError($"Failed to save settings: {ex.Message}");
      }
      Pause();
    }

    private static void ShowRealmSelectMenu(AppSettings settings)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Set it on home screen.");
        Pause();
        return;
      }

      var list = ScanGuids(settings.FolderPath!, "Realm");
      Console.Clear();
      DrawHeader("=== Select Realm ===", ConsoleColor.Blue);
      if (list.Count == 0)
      {
        WriteWarn("No Realm GUIDs found in filenames.");
        Pause();
        return;
      }

      for (int i = 0; i < list.Count; i++)
      {
        var guid = list[i];
        var name = settings.RealmNames != null && settings.RealmNames.TryGetValue(guid, out var n) ? n : null;
        Console.WriteLine($"{i + 1}. {(name is not null ? name + " - " : string.Empty)}{guid}");
      }
      Console.WriteLine();
      Console.Write("Enter number to select (or blank to cancel): ");
      var input = Console.ReadLine()?.Trim();
      if (string.IsNullOrWhiteSpace(input)) return;
      if (!int.TryParse(input, out var idx) || idx < 1 || idx > list.Count)
      {
        WriteError("Invalid selection.");
        Pause();
        return;
      }
      var selected = list[idx - 1];
      settings.RealmGuid = selected;

      Console.Write("Name? (optional, leave blank to skip): ");
      var nameInput = Console.ReadLine()?.Trim();
      if (!string.IsNullOrWhiteSpace(nameInput))
      {
        settings.RealmNames ??= new Dictionary<string, string>();
        settings.RealmNames[selected] = nameInput;
      }

      try
      {
        new SettingsManager().Save(settings);
        WriteSuccess("Realm selection saved.");
      }
      catch (Exception ex)
      {
        WriteError($"Failed to save settings: {ex.Message}");
      }
      Pause();
    }

    private static List<string> ScanGuids(string workingDir, string kind)
    {
      var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      try
      {
        var files = Directory.GetFiles(workingDir);
        string pattern = kind.Equals("Character", StringComparison.OrdinalIgnoreCase)
          ? @"Character[_\- ](?<g>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})"
          : @"Realm[_\- ](?<g>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})";
        var rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (var f in files)
        {
          var name = Path.GetFileName(f);
          var m = rx.Match(name);
          if (m.Success)
          {
            var g = m.Groups["g"].Value;
            if (!string.IsNullOrWhiteSpace(g)) results.Add(g);
          }
        }
      }
      catch
      {
        // ignore scanning errors
      }
      return results.OrderBy(x => x).ToList();
    }

    private static bool ConfirmRestoreIfOld(string archiveFullPath)
    {
      try
      {
        var fi = new FileInfo(archiveFullPath);
        var age = DateTime.UtcNow - fi.LastWriteTimeUtc;
        if (age > TimeSpan.FromMinutes(5))
        {
          WriteWarn($"Backup '{fi.Name}' is {age.TotalMinutes:F1} minutes old. Proceed with restore? (Y/N)");
          while (true)
          {
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Y) return true;
            if (key == ConsoleKey.N) return false;
          }
        }
      }
      catch
      {
        // If we can't read file info, proceed without the warning
      }
      return true;
    }
  }
}