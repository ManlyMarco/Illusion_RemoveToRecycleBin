using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using Illusion.Extensions;

namespace KK_RemoveToRecycleBin
{
    [BepInPlugin(GUID, "Remove Cards To Recycle Bin", Version)]
    public class RemoveToRecycleBin : BaseUnityPlugin
    {
        public const string GUID = "marco.RemoveToRecycleBin";
        public const string Version = "1.1";

        private static ManualLogSource _logger;

        private void Start()
        {
            _logger = Logger;

            // Only use of ass-csharp, universal across games. Use Start instead of Awake bacuse of this
            _fullUserDataPath = Path.GetFullPath(UserData.Path);

            var h = HarmonyWrapper.PatchAll(typeof(RemoveToRecycleBin));

            // Patch all FileStream to account for differences in internals of different framework versions
            var hook = new HarmonyMethod(typeof(RemoveToRecycleBin), nameof(FileStreamHook));
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
            if (!File.Exists(path)) return;

            try
            {
                var fullPath = Path.GetFullPath(path);
                if (fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    fullPath.StartsWith(_fullUserDataPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!RecycleBinUtil.MoveToRecycleBin(fullPath))
                        throw new Exception("Call returned false, check if recycle bin is enabled");
                    _logger.Log(LogLevel.Info, $"Moved \"{fullPath}\" to recycle bin before it was removed or overwritten.");
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning, $"Failed to move \"{path}\" to recycle bin, it will be permanently deleted or overwritten.\n{e}");
            }
        }
    }
}
