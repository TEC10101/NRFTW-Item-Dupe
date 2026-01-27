using System.Text;
using System.Diagnostics;

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
            Console.Write("\nEnter folder path: ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
              WriteInfo("No path entered. Keeping existing value.");
              break;
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
            break;

          case ConsoleKey.F2:
            ShowExtractMenu(settings);
            break;

          case ConsoleKey.F3:
            ShowArchiveMenu(settings);
            break;

          case ConsoleKey.F4:
            WriteInfo("F4 pressed: placeholder command to be implemented.");
            // TODO: Implement command execution for F4
            Pause();
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
      Console.WriteLine("=== Duplicator ===");
      Console.WriteLine();
      Console.WriteLine($"Current Folder: {settings.FolderPath ?? "<not set>"}");
      Console.WriteLine();
      Console.WriteLine("F1 - Set Folder Path");
      Console.WriteLine("F2 - Extract RAR (Character/Realm)");
      Console.WriteLine("F3 - Create RAR (Character/Realm)");
      Console.WriteLine("F4 - Execute Command 3 (TBD)");
      Console.WriteLine("ESC/Q - Quit");
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
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(message);
      Console.ForegroundColor = prev;
    }

    private static void WriteInfo(string message)
    {
      Console.WriteLine(message);
    }

    private static void ShowArchiveMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        Console.WriteLine("=== Create Archive ===");
        Console.WriteLine();
        Console.WriteLine("1 - Character");
        Console.WriteLine("2 - Realm");
        Console.WriteLine();
        Console.WriteLine("Press F3 to return to main menu.");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F3:
            return; // back to main menu
          case ConsoleKey.D1:
          case ConsoleKey.NumPad1:
            RunWinRarAdd(settings, "Character.rar", CharacterGuid);
            break;
          case ConsoleKey.D2:
          case ConsoleKey.NumPad2:
            RunWinRarAdd(settings, "Realm.rar", RealmGuid);
            break;
          default:
            // ignore other keys
            break;
        }
      }
    }

    private static void RunWinRarAdd(AppSettings settings, string archiveName, string guid)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Press F1 to set it first.");
        Pause();
        return;
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
        return;
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
          return;
        }

        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode == 0)
        {
          WriteSuccess($"Archive '{archiveName}' created successfully.");
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
    }

    private static void ShowExtractMenu(AppSettings settings)
    {
      while (true)
      {
        Console.Clear();
        Console.WriteLine("=== Extract Archive ===");
        Console.WriteLine();
        Console.WriteLine("1 - Character");
        Console.WriteLine("2 - Realm");
        Console.WriteLine();
        Console.WriteLine("Press F2 to return to main menu.");

        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
          case ConsoleKey.F2:
            return; // back to main menu
          case ConsoleKey.D1:
          case ConsoleKey.NumPad1:
            RunWinRarExtract(settings, "Character.rar");
            break;
          case ConsoleKey.D2:
          case ConsoleKey.NumPad2:
            RunWinRarExtract(settings, "Realm.rar");
            break;
          default:
            // ignore other keys
            break;
        }
      }
    }

    private static void RunWinRarExtract(AppSettings settings, string archiveName)
    {
      if (string.IsNullOrWhiteSpace(settings.FolderPath))
      {
        WriteError("Folder path not set. Press F1 to set it first.");
        Pause();
        return;
      }

      var workingDir = settings.FolderPath!;
      var archivePath = Path.Combine(workingDir, archiveName);
      if (!File.Exists(archivePath))
      {
        WriteError($"{archiveName} not found in the selected folder.");
        Pause();
        return;
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
        return;
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
          return;
        }

        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode == 0)
        {
          WriteSuccess("Extraction completed successfully.");
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
    }
  }
}