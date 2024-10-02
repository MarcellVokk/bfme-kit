using BfmeFoundationProject.RegistryKit.Data;
using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace BfmeFoundationProject.RegistryKit
{
    [SupportedOSPlatform("windows")]
    public static class BfmeRegistryManager
    {
        public static string GetKeyValue(object game, BfmeRegistryKey key)
        {
            string valueRegistryKey = key switch
            {
                BfmeRegistryKey.InstallPath => "InstallPath",
                BfmeRegistryKey.Language => "Language",
                BfmeRegistryKey.MapPackVersion => "MapPackVersion",
                BfmeRegistryKey.UseLocalUserMaps => "UseLocalUserMaps",
                BfmeRegistryKey.UserDataLeafName => "UserDataLeafName",
                BfmeRegistryKey.Version => "Version",
                _ => ""
            };
            string gameRegistryKeySuffix = key switch
            {
                BfmeRegistryKey.SerialKey => @"\ergc",
                _ => ""
            };

            EnsureFixedRegistry(game);

            if (key == BfmeRegistryKey.InstallPath)
            {
                using RegistryKey? legacyRegistryKey = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\{BfmeDefaults.DeprecatedGameRegistryKeys[Convert.ToInt32(game)]}", false);
                string legacyInstallDir = "";
                if (legacyRegistryKey != null) legacyInstallDir = legacyRegistryKey?.GetValue("Install Dir") as string ?? "";
                if (legacyInstallDir != "") return legacyInstallDir;
            }

            using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\{BfmeDefaults.DefaultGameRegistryKeys[Convert.ToInt32(game)]}{gameRegistryKeySuffix}", false);
            string result = registryKey?.GetValue(valueRegistryKey) as string ?? "";
            return result;
        }

        public static void SetKeyValue(object game, BfmeRegistryKey key, string value, RegistryValueKind valueType = RegistryValueKind.String)
        {
            string valueRegistryKey = key switch
            {
                BfmeRegistryKey.InstallPath => "InstallPath",
                BfmeRegistryKey.Language => "Language",
                BfmeRegistryKey.MapPackVersion => "MapPackVersion",
                BfmeRegistryKey.UseLocalUserMaps => "UseLocalUserMaps",
                BfmeRegistryKey.UserDataLeafName => "UserDataLeafName",
                BfmeRegistryKey.Version => "Version",
                BfmeRegistryKey.SerialKey => "",
                _ => ""
            };
            string gameRegistryKeySuffix = key switch
            {
                BfmeRegistryKey.SerialKey => @"\ergc",
                _ => ""
            };

            using RegistryKey? registryKey = Registry.LocalMachine.CreateSubKey(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\{BfmeDefaults.DefaultGameRegistryKeys[Convert.ToInt32(game)]}{gameRegistryKeySuffix}", true);
            registryKey?.SetValue(valueRegistryKey, value, valueType);
        }

        public static void CreateNewInstallRegistry(object game, string installPath, string language)
        {
            if (!Path.EndsInDirectorySeparator(installPath)) installPath += Path.DirectorySeparatorChar;
            if (!Directory.Exists(installPath)) Directory.CreateDirectory(installPath);

            EnsureFixedRegistry(game);
            SetKeyValue(game, BfmeRegistryKey.InstallPath, installPath);
            SetKeyValue(game, BfmeRegistryKey.Language, language);
            SetKeyValue(game, BfmeRegistryKey.MapPackVersion, "65536", RegistryValueKind.DWord);
            SetKeyValue(game, BfmeRegistryKey.UseLocalUserMaps, "0", RegistryValueKind.DWord);
            SetKeyValue(game, BfmeRegistryKey.UserDataLeafName, BfmeDefaults.DefaultUserDataLeafNames[Convert.ToInt32(game)]);
            SetKeyValue(game, BfmeRegistryKey.Version, "65539", RegistryValueKind.DWord);
            SetKeyValue(game, BfmeRegistryKey.SerialKey, new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 20).Select(s => s[Random.Shared.Next(s.Length)]).ToArray()));
            EnsureDefaults(game);
        }

        public static void EnsureDefaults(object game)
        {
            Registry.LocalMachine.DeleteSubKeyTree(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\Microsoft\Windows\CurrentVersion\App Paths\{BfmeDefaults.DefaultGameExecutableNames[Convert.ToInt32(game)]}", false);
            using RegistryKey? keyApp = Registry.LocalMachine.CreateSubKey(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\Microsoft\Windows\CurrentVersion\App Paths\{BfmeDefaults.DefaultGameExecutableNames[Convert.ToInt32(game)]}", true);
            keyApp?.SetValue("", Path.Combine(GetKeyValue(game, BfmeRegistryKey.InstallPath), BfmeDefaults.DefaultGameExecutableNames[Convert.ToInt32(game)]));
            keyApp?.SetValue("Game Registry", @$"SOFTWARE\{BfmeDefaults.DefaultGameRegistryKeys[Convert.ToInt32(game)]}");
            keyApp?.SetValue("Installed", "1", RegistryValueKind.DWord);
            keyApp?.SetValue("Path", GetKeyValue(game, BfmeRegistryKey.InstallPath));
            keyApp?.SetValue("Restart", "0", RegistryValueKind.DWord);

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetKeyValue(game, BfmeRegistryKey.UserDataLeafName))))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetKeyValue(game, BfmeRegistryKey.UserDataLeafName)));

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetKeyValue(game, BfmeRegistryKey.UserDataLeafName), "Options.ini")) || File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetKeyValue(game, BfmeRegistryKey.UserDataLeafName), "Options.ini")).Length <= 6)
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetKeyValue(game, BfmeRegistryKey.UserDataLeafName), "Options.ini"), BfmeDefaults.DefaultOptions);

            if (game.Equals(2))
                EnsureDefaults(1);
        }

        public static void EnsureFixedRegistry(object game)
        {
            try
            {
                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\{BfmeDefaults.DeprecatedGameRegistryKeys[Convert.ToInt32(game)]}", false);
                    if (registryKey == null) return;
                    SetKeyValue(game, BfmeRegistryKey.InstallPath, registryKey.GetValue("Install Dir")?.ToString() ?? "");
                    Registry.LocalMachine.DeleteSubKeyTree(@$"SOFTWARE\{(nint.Size == 8 ? "WOW6432Node" : "")}\{BfmeDefaults.DeprecatedGameRegistryKeys[Convert.ToInt32(game)]}", false);
                }
            }
            catch { }
        }

        public static bool IsInstalled(object game) => GetKeyValue(game, BfmeRegistryKey.InstallPath) != "" && Directory.Exists(GetKeyValue(game, BfmeRegistryKey.InstallPath));

        public static int GameNameToInt(string gameName)
        {
            return gameName.ToUpper() switch
            {
                "BFME1" => 0,
                "BFME2" => 1,
                "ROTWK" => 2,
                _ => 0
            };
        }
    }
}
