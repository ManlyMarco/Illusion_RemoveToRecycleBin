using System;
using System.IO;
using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using Harmony;

namespace KK_RemoveToRecycleBin
{
    [BepInPlugin(GUID, "Move removed/overwritten cards to recycle bin", Version)]
    public class RemoveToRecycleBin : BaseUnityPlugin
    {
        public const string GUID = "marco.RemoveToRecycleBin";
        internal const string Version = "1.0";

        private static ManualLogSource _logger;

        private void Awake()
        {
            _logger = Logger; 
            _fullUserDataPath = Path.GetFullPath(UserData.Path);
            HarmonyWrapper.PatchAll(typeof(RemoveToRecycleBin));
        }

        private static string _fullUserDataPath;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileStream), MethodType.Constructor, typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(int), typeof(bool), typeof(FileOptions))]
        public static void FileStreamHook(string path, FileMode mode, FileAccess access)
        {
            if (mode == FileMode.Create && access.HasFlag(FileAccess.Write))
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

            var fullPath = Path.GetFullPath(path);
            if (fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                fullPath.StartsWith(_fullUserDataPath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!RecycleBinUtil.MoveToRecycleBin(fullPath))
                        throw new Exception("Call returned false, check if recycle bin is enabled");
                    _logger.Log(LogLevel.Info, $"Moved \"{fullPath}\" to recycle bin before it was removed or overwritten.");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Warning, $"Failed to move \"{fullPath}\" to recycle bin, it will be permanently deleted or overwritten.\n{e}");
                }
            }
        }
    }
}
