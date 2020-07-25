using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Options;
using UnityEngine;

namespace EnhancedSurvival
{
    class SettingsManager
    {
        private static ModOptions options;

        internal static KeyCode keySleepStart { get; private set; } = KeyCode.N;
        internal static KeyCode keySleepStop { get; private set; } = KeyCode.N;

        internal static void Initialize()
        {
            if (options != null) return;
            options = new ModOptions();
            SMLHelper.V2.Handlers.OptionsPanelHandler.RegisterModOptions(options);
            Load();
        }

        internal static string GetConfigFolder()
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            return assemblyDirectory;
        }
        internal static string GetConfigFile()
        {
            string saveFile = GetConfigFolder() + "/config.json";
            return saveFile;
        }
        internal static string GetKeybindsFile()
        {
            string saveFile = GetConfigFolder() + "/keybinds.json";
            return saveFile;
        }
        internal static void Load()
        {
            keySleepStart = KeyCode.N;
            keySleepStop = KeyCode.N;
            if (File.Exists(GetKeybindsFile())) {
                Dictionary<string, int> keybinds = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(GetKeybindsFile()));
                if (keybinds.ContainsKey("sleepStart")) keySleepStart = (KeyCode) keybinds["sleepStart"];
                if (keybinds.ContainsKey("sleepStop")) keySleepStop = (KeyCode) keybinds["sleepStop"];
            }
        }
        internal static void Save()
        {
            Dictionary<string, int> keybinds = new Dictionary<string, int>();
            keybinds.Add("sleepStart", (int)keySleepStart);
            keybinds.Add("sleepStop", (int)keySleepStop);
            File.WriteAllText(GetKeybindsFile(), JsonConvert.SerializeObject(keybinds));
        }

        public class ModOptions : SMLHelper.V2.Options.ModOptions
        {
            public ModOptions(string name = "Enhanced Survival") : base(name)
            {
                this.KeybindChanged += OnKeybindChanged;
            }

            private void OnKeybindChanged(object sender, KeybindChangedEventArgs e)
            {
                if (e.Id == "sleepStart") {
                    keySleepStart = e.Key;
                    Save();
                } else if (e.Id == "sleepStop") {
                    keySleepStop = e.Key;
                    Save();
                }
            }

            public override void BuildModOptions()
            {
                this.AddKeybindOption("sleepStart", "Start sleeping", GameInput.Device.Keyboard, keySleepStart);
                this.AddKeybindOption("sleepStop", "Stop sleeping", GameInput.Device.Keyboard, keySleepStop);
            }
        }
    }
}
