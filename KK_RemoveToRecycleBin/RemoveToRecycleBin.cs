using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using Illusion.Extensions;

namespace KK_RemoveToRecycleBin
{
    [BepInPlugin(GUID, "Move removed/overwritten cards to recycle bin", Version)]
    public class RemoveToRecycleBin : BaseUnityPlugin
    {
        public const string GUID = "marco.RemoveToRecycleBin";
        internal const string Version = "1.0";

        private void Start()
        {
            _fullUserDataPath = Path.GetFullPath(UserData.Path);
            HarmonyInstance.Create(GUID).PatchAll(typeof(RemoveToRecycleBin));
        }

        private static string _fullUserDataPath;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileStream), null, new[] { typeof(string), typeof(FileMode), typeof(FileAccess) })]
        public static void FileStreamHook(string path, FileMode mode, FileAccess access)
        {
            if (mode == FileMode.Create && access.HasFlag(FileAccess.Write))
                MaybeDelete(path);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(File), nameof(File.Delete), new[] { typeof(string) })]
        public static void FileDeleteHook(string path)
        {
            MaybeDelete(path);
        }

        private static void MaybeDelete(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var fullPath = Path.GetFullPath(path);
            if (fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                fullPath.StartsWith(_fullUserDataPath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!RecycleBinUtil.MoveToRecycleBin(fullPath))
                        throw new Exception("Call returned false, check if recycle bin is enabled");
                    Logger.Log(LogLevel.Info, $"Moved \"{fullPath}\" to recycle bin before it was removed or overwritten.");
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Warning, $"Failed to move \"{fullPath}\" to recycle bin, it will be permanently deleted or overwritten.\n{e}");
                }
            }
        }
    }
}
