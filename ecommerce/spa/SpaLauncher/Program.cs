using System.Diagnostics;

var spaDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

if (!Directory.Exists(spaDirectory))
{
    Console.Error.WriteLine($"SPA directory was not found: {spaDirectory}");
    return;
}

try
{
    Process.Start(new ProcessStartInfo
    {
        FileName = "cmd.exe",
        Arguments = "/c start \"Aura SPA\" npm run dev",
        WorkingDirectory = spaDirectory,
        UseShellExecute = true
    });

    Console.WriteLine($"SPA dev server launch requested in {spaDirectory}");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Failed to start SPA dev server: {exception.Message}");
}
