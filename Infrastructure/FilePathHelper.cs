using System;
using System.IO;

namespace MSFSGraphicsPresetSwitcher.Infrastructure
{
    public static class FilePathHelper
    {
        public static string? AutoDetectUserCfgPath()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                string msfsPath = Path.Combine(appData, "Microsoft Flight Simulator 2024", "UserCfg.opt");

                if (File.Exists(msfsPath))
                    return msfsPath;

                return null;
            }
            catch
            {
                throw new Exception();
            }
        }

        public static string? AutoDetectPresetFolder()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string presetFolder = Path.Combine(appData, "MSFSGraphicsPresets");

                Directory.CreateDirectory(presetFolder);

                return presetFolder;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
