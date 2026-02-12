using MSFSGraphicsPresetSwitcher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSFSGraphicsPresetSwitcher.Services
{
    public class GraphicsPresetService
    {
        public List<GraphicsPresetEntry> ListPresets(string presetFolder)
        {
            if (string.IsNullOrWhiteSpace(presetFolder) || !Directory.Exists(presetFolder))
                return new List<GraphicsPresetEntry>();

            return Directory.GetFiles(presetFolder, "*.opt")
                            .Select(Path.GetFileName)
                            .Select(f => new GraphicsPresetEntry { Name = f ?? "Preset name missing..." })
                            .ToList();
        }

        public void SavePreset(string liveFilePath, string presetFolder, string presetName)
        {
            if (!File.Exists(liveFilePath))
                throw new FileNotFoundException("Live config not found", liveFilePath);

            if (!Directory.Exists(presetFolder))
                Directory.CreateDirectory(presetFolder);

            string presetPath = Path.Combine(presetFolder, presetName + ".opt");
            File.Copy(liveFilePath, presetPath, overwrite: true);
        }

        public void ActivatePreset(GraphicsPresetEntry preset, string liveFilePath, string presetFolder)
        {
            string presetPath = Path.Combine(presetFolder, preset.Name);
            if (!File.Exists(presetPath))
                throw new FileNotFoundException("Preset not found", presetPath);

            File.Copy(presetPath, liveFilePath, overwrite: true);
        }

        public void RemovePreset(GraphicsPresetEntry preset, string presetFolder)
        {
            string presetPath = Path.Combine(presetFolder, preset.Name);
            if (File.Exists(presetPath))
                File.Delete(presetPath);
        }

        public void BackupLiveConfig(string liveFilePath, string presetFolder)
        {
            if (!File.Exists(liveFilePath))
                throw new FileNotFoundException("Live config not found", liveFilePath);

            if (!Directory.Exists(presetFolder))
                Directory.CreateDirectory(presetFolder);

            string backupPath = Path.Combine(presetFolder, "UserCfg.opt.bkp");
            if (!File.Exists(backupPath))
                File.Copy(liveFilePath, backupPath);
        }
    }
}
