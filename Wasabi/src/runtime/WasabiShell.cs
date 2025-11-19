using System.Diagnostics;

namespace Wasabi.runtime;

/// <summary>
/// Provides shell command execution functionality for Wasabi scripts.
/// </summary>
public static class WasabiShell
{
    /// <summary>
    /// Executes a shell command using bash.
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <returns>True if the command executed successfully, false otherwise</returns>
    public static bool Execute(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };
            
            process.Start();
            process.WaitForExit();
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WASABI ERROR] Shell execution failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Executes a shell command and captures its output.
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <returns>The command output, or null if execution failed</returns>
    public static string? ExecuteWithOutput(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return process.ExitCode == 0 ? output : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WASABI ERROR] Shell execution failed: {ex.Message}");
            return null;
        }
    }
}

