using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace RemoveToRecycleBin
{
    [BepInPlugin(GUID, "Remove Cards To Recycle Bin", Version)]
    public class RemoveToRecycleBinPlugin : BaseUnityPlugin
    {
        public const string GUID = "marco.RemoveToRecycleBin";
        public const string Version = "2.0";

        private static ManualLogSource _logger;

        private void Start()
        {
            _logger = Logger;

            // UserData is universal across games. Run in Start to let the game create the dir. Don't use UserData.Path since it's broken in EC
            _fullUserDataPath = Path.GetFullPath(Path.Combine(Paths.GameRootPath, "UserData\\")).ToLower();

            var h = Harmony.CreateAndPatchAll(typeof(RemoveToRecycleBinPlugin), GUID);

            // Patch all FileStream to account for differences in internals of different framework versions
            var hook = new HarmonyMethod(typeof(RemoveToRecycleBinPlugin), nameof(FileStreamHook));
            if (hook == null) throw new ArgumentNullException(nameof(hook));
            foreach (var m in AccessTools.GetDeclaredConstructors(typeof(FileStream)))
            {
                var args = m.GetParameters();
                if (args.Any(x => x.ParameterType == typeof(FileMode)) && args.Any(x => x.ParameterType == typeof(FileAccess)))
                {
                    h.Patch(m, hook);
                    //_logger.LogDebug("Patching " + m);
                }
            }
        }

        private static string _fullUserDataPath;

        public static void FileStreamHook(string path, FileMode mode, FileAccess access)
        {
            if (mode == FileMode.Create && (access & FileAccess.Write) == FileAccess.Write)
                MoveToRecycleBin(path);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(File), nameof(File.Delete), new[] { typeof(string) })]
        public static void FileDeleteHook(string path)
        {
            MoveToRecycleBin(path);
        }

        private static void MoveToRecycleBin(string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                var fullPath = Path.GetFullPath(path).ToLower();
                if (fullPath.EndsWith(".png", StringComparison.Ordinal) &&
                    fullPath.StartsWith(_fullUserDataPath, StringComparison.Ordinal))
                {
                    var relativePath = fullPath.Substring(_fullUserDataPath.Length);

                    // chara, coordinate and studio/scene are in all games; map, pose and edit are in EC
                    if (relativePath.StartsWith("chara\\") || relativePath.StartsWith("coordinate\\") || relativePath.StartsWith("studio\\scene\\") || 
                        relativePath.StartsWith("map\\") || relativePath.StartsWith("pose\\") || relativePath.StartsWith("edit\\"))
                    {
                        if (!RecycleBinUtil.MoveToRecycleBin(fullPath))
                            throw new Exception("Call returned false, check if recycle bin is enabled");
                        _logger.Log(LogLevel.Info, $"Moved \"{fullPath}\" to recycle bin before it was removed or overwritten.");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning, $"Failed to move \"{path}\" to recycle bin, it will be permanently deleted or overwritten.\n{e}");
            }
        }
    }
}
