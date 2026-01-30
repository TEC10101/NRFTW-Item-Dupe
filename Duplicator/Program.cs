using System.Text;
using System.Diagnostics;
using System.Linq;

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
            ShowConfigMenu(settingsManager, settings);
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
      DrawLineAt(GetRowForFKey(ConsoleKey.F9), "F9 - Update Config ▶");
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
      Console.WriteLine("=== Update Folder Path ===");
      Console.WriteLine();
      Console.WriteLine($"Current: {settings.FolderPath ?? "<not set>"}");
      Console.Write("New path (leave blank to cancel): ");

      var input = Console.ReadLine()?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(input))
      {
        WriteInfo("No changes made.");
        Pause();
        return;
      }

      settings.FolderPath = input;
      try
      {
        settingsManager.Save(settings);
        if (!Directory.Exists(input))
        {
          WriteWarn("Path saved, but directory does not exist.");
        }
        else
        {
          WriteSuccess("Folder path saved.");
        }
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
        DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F2), "F2 - Backup Character Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F3), "F3 - Restore Character Data");

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
        DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F6), "F6 - Backup Realm Data");
        DrawLineAt(GetRowForFKey(ConsoleKey.F7), "F7 - Restore Realm Data");

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
        DrawHeader("=== Character ===", ConsoleColor.Yellow);
        DrawLineAt(GetRowForFKey(ConsoleKey.F1), "F1 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F2), "F2 - Backup Character Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F1:
            return; // back to Character submenu
          case ConsoleKey.F2:
            {
              var archiveName = GetDatedArchiveName("Character");
              var ok = RunWinRarAdd(settings, archiveName, GetCharacterGuid(settings));
              if (ok)
              {
                ShowSuccessAndWait("Character backup completed");
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
        DrawHeader("=== Realm ===", ConsoleColor.Blue);
        DrawLineAt(GetRowForFKey(ConsoleKey.F5), "F5 - Go Back");
        DrawLineAt(GetRowForFKey(ConsoleKey.F6), "F6 - Backup Realm Data");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F5:
            return; // back to Realm submenu
          case ConsoleKey.F6:
            {
              var archiveName = GetDatedArchiveName("Realm");
              var ok = RunWinRarAdd(settings, archiveName, GetRealmGuid(settings));
              if (ok)
              {
                ShowSuccessAndWait("Realm backup completed");
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
        DrawHeader("=== Character ===", ConsoleColor.Yellow);
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
              var latest = FindLatestArchive(workingDir, "Character");
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
              if (ok) ShowSuccessAndWait("Character restore completed");
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
        DrawHeader("=== Realm ===", ConsoleColor.Blue);
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
              var latest = FindLatestArchive(workingDir, "Realm");
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
              if (ok) ShowSuccessAndWait("Realm restore completed");
            }
            break;
          default:
            break;
        }
      }
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
        ConsoleKey.F10 => 13,
        ConsoleKey.F11 => 14,
        ConsoleKey.F12 => 15,
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
          WriteSuccess("Extraction completed successfully.");
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

    private static string GetDatedArchiveName(string baseName)
    {
      var date = DateTime.Now.ToString("yyyy-MM-dd");
      return $"{date}_{baseName}.rar";
    }

    private static string? FindLatestArchive(string workingDir, string baseName)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
        {
          return null;
        }
        var pattern = $"*_{baseName}.rar";
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

    private static string GetCharacterGuid(AppSettings settings)
    {
      return string.IsNullOrWhiteSpace(settings.CharacterGuid) ? CharacterGuid : settings.CharacterGuid!;
    }

    private static string GetRealmGuid(AppSettings settings)
    {
      return string.IsNullOrWhiteSpace(settings.RealmGuid) ? RealmGuid : settings.RealmGuid!;
    }

    private static void ShowConfigMenu(SettingsManager settingsManager, AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        DrawHeader("=== Update Config ===", null);
        DrawLineAt(GetRowForFKey(ConsoleKey.F9), "F9 - Back to Home");
        DrawLineAt(GetRowForFKey(ConsoleKey.F10), "F10 - Update Character GUID");
        DrawLineAt(GetRowForFKey(ConsoleKey.F11), "F11 - Update Realm GUID");
        DrawLineAt(GetRowForFKey(ConsoleKey.F12), "F12 - Update Folder Path");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F9:
            return; // back to home
          case ConsoleKey.F10:
            UpdateCharacterGuidScreen(settingsManager, settings);
            break;
          case ConsoleKey.F11:
            UpdateRealmGuidScreen(settingsManager, settings);
            break;
          case ConsoleKey.F12:
            UpdateFolderPathScreen(settingsManager, settings);
            break;
          default:
            break;
        }
      }
    }

    private static void UpdateCharacterGuidScreen(SettingsManager settingsManager, AppSettings settings)
    {
      Console.Clear();
      Console.WriteLine("=== Update Character GUID ===");
      Console.WriteLine();
      Console.WriteLine($"Current: {settings.CharacterGuid ?? "<not set>"}");
      Console.Write("New GUID (leave blank to cancel): ");

      var input = Console.ReadLine()?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(input))
      {
        WriteInfo("No changes made.");
        Pause();
        return;
      }

      settings.CharacterGuid = input;
      try
      {
        settingsManager.Save(settings);
        WriteSuccess("Character GUID saved.");
      }
      catch (Exception ex)
      {
        WriteError($"Failed to save settings: {ex.Message}");
      }
      Pause();
    }

    private static void UpdateRealmGuidScreen(SettingsManager settingsManager, AppSettings settings)
    {
      Console.Clear();
      Console.WriteLine("=== Update Realm GUID ===");
      Console.WriteLine();
      Console.WriteLine($"Current: {settings.RealmGuid ?? "<not set>"}");
      Console.Write("New GUID (leave blank to cancel): ");

      var input = Console.ReadLine()?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(input))
      {
        WriteInfo("No changes made.");
        Pause();
        return;
      }

      settings.RealmGuid = input;
      try
      {
        settingsManager.Save(settings);
        WriteSuccess("Realm GUID saved.");
      }
      catch (Exception ex)
      {
        WriteError($"Failed to save settings: {ex.Message}");
      }
      Pause();
    }
  }
}