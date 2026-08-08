using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MGSC;
using ModConfigMenu;
using ModConfigMenu.Contracts;
using ModConfigMenu.Implementations;
using ModConfigMenu.Objects;
using ModConfigMenu.Services;
using UnityEngine;

namespace GatherEntireFloorLoot
{
    // Token: 0x02000004 RID: 4
    public class ModConfigData
    {
        // Token: 0x0600000E RID: 14 RVA: 0x0000221C File Offset: 0x0000041C
        public ModConfigData(string ConfigPath)
        {
            this.ConfigPath = ConfigPath;
            this.Settings = new Dictionary<string, object>();
            this.ConfigValues = new List<IConfigValue>();
            this.LoadConfig();
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002247 File Offset: 0x00000447
        public void RegisterModConfigData(string menuName)
        {
            ModConfigMenuAPI.RegisterModConfig(menuName, this.ConfigValues, new ModConfigMenuAPI.ConfigStoredDelegate(this.OnSave));
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002262 File Offset: 0x00000462
        public void AddConfigHeader(string headerKey, string locKey = null)
        {
            this.GetKeyEnsureLocalization(headerKey, ModConfigData.KeyType.Header, locKey);
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002270 File Offset: 0x00000470
        public void AddConfigValue(string headerKey, string valueKey, string stringKey)
        {
            string keyEnsureLocalization = this.GetKeyEnsureLocalization(stringKey, ModConfigData.KeyType.Description, valueKey);
            StringConfig item = new StringConfig(valueKey, keyEnsureLocalization, headerKey);
            this.ConfigValues.Add(item);
        }

        // Token: 0x06000012 RID: 18 RVA: 0x0000229C File Offset: 0x0000049C
        public void AddConfigValue(string headerKey, string valueKey, object defaultValue, string labelKey, string tooltipKey)
        {
            string keyEnsureLocalization = this.GetKeyEnsureLocalization(headerKey, ModConfigData.KeyType.Header, valueKey);
            string keyEnsureLocalization2 = this.GetKeyEnsureLocalization(labelKey, ModConfigData.KeyType.Label, valueKey);
            string keyEnsureLocalization3 = this.GetKeyEnsureLocalization(tooltipKey, ModConfigData.KeyType.Tooltip, valueKey);
            if (!this.Settings.ContainsKey(valueKey))
            {
                this.Settings.Add(valueKey, defaultValue);
            }
            ConfigValue item = new ConfigValue(valueKey, this.Settings[valueKey], keyEnsureLocalization, defaultValue, keyEnsureLocalization3, keyEnsureLocalization2);
            this.ConfigValues.Add(item);
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002308 File Offset: 0x00000508
        public void AddConfigValue(string headerKey, string valueKey, int defaultValue, int min, int max, string labelKey, string tooltipKey)
        {
            string keyEnsureLocalization = this.GetKeyEnsureLocalization(headerKey, ModConfigData.KeyType.Header, valueKey);
            string keyEnsureLocalization2 = this.GetKeyEnsureLocalization(labelKey, ModConfigData.KeyType.Label, valueKey);
            string keyEnsureLocalization3 = this.GetKeyEnsureLocalization(tooltipKey, ModConfigData.KeyType.Tooltip, valueKey);
            if (!this.Settings.ContainsKey(valueKey))
            {
                this.Settings.Add(valueKey, defaultValue);
            }
            RangeConfig<int> item = new RangeConfig<int>(valueKey, this.GetConfigValue<int>(valueKey, 0), defaultValue, min, max, keyEnsureLocalization, keyEnsureLocalization3, keyEnsureLocalization2);
            this.ConfigValues.Add(item);
        }



        // Token: 0x06000014 RID: 20 RVA: 0x00002378 File Offset: 0x00000578
        public void AddConfigValue(string headerKey, string valueKey, string defaultValue, List<object> valueList, string labelKey, string tooltipKey)
        {
            string keyEnsureLocalization = this.GetKeyEnsureLocalization(headerKey, ModConfigData.KeyType.Header, valueKey);
            string keyEnsureLocalization2 = this.GetKeyEnsureLocalization(labelKey, ModConfigData.KeyType.Label, valueKey);
            string keyEnsureLocalization3 = this.GetKeyEnsureLocalization(tooltipKey, ModConfigData.KeyType.Tooltip, valueKey);
            if (!this.Settings.ContainsKey(valueKey))
            {
                this.Settings.Add(valueKey, defaultValue);
            }
            DropdownConfig item = new DropdownConfig(valueKey, this.GetConfigValue<string>(valueKey, null), keyEnsureLocalization, defaultValue, keyEnsureLocalization3, keyEnsureLocalization2, valueList);
            this.ConfigValues.Add(item);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000023E4 File Offset: 0x000005E4
        public T GetConfigValue<T>(string key, T fallback = default(T))
        {
            object value;
            if (this.Settings.TryGetValue(key, out value))
            {
                try
                {
                    return (T)((object)Convert.ChangeType(value, typeof(T)));
                }
                catch
                {
                    return fallback;
                }
                return fallback;
            }
            return fallback;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002434 File Offset: 0x00000634
        public T GetDropdownValue<T>(string key, T fallback = default(T))
        {
            object obj;
            if (this.Settings.TryGetValue(key, out obj))
            {
                try
                {
                    return (T)((object)Convert.ChangeType(obj, typeof(T)));
                }
                catch
                {
                    string text = obj as string;
                    if (!string.IsNullOrEmpty(text))
                    {
                        Match match = Regex.Match(text, "^(\\d+)\\.");
                        int num;
                        if (match.Success && int.TryParse(match.Groups[1].Value, out num))
                        {
                            num--;
                            try
                            {
                                return (T)((object)Convert.ChangeType(num, typeof(T)));
                            }
                            catch
                            {
                                return fallback;
                            }
                        }
                    }
                    return fallback;
                }
                return fallback;
            }
            return fallback;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x000024F8 File Offset: 0x000006F8
        public TEnum GetEnumValue<TEnum>(string key, TEnum fallback = default(TEnum)) where TEnum : struct, Enum
        {
            Debug.Log("START " + key);
            string configValue = this.GetConfigValue<string>(key, null);
            if (string.IsNullOrEmpty(configValue))
            {
                return fallback;
            }
            TEnum result;
            try
            {
                Debug.Log("try start");
                int num = configValue.IndexOf('.');
                int num2;
                if (num <= 0)
                {
                    Debug.Log("RETURN INDEX DOT");
                    result = fallback;
                }
                else if (int.TryParse(configValue.Substring(0, num), out num2))
                {
                    Debug.Log("PRASED numberPart");
                    num2--;
                    TEnum[] array = (TEnum[])Enum.GetValues(typeof(TEnum));
                    if (num2 < 0)
                    {
                        Debug.Log("index < 0");
                        result = fallback;
                    }
                    else if (num2 >= array.Length)
                    {
                        Debug.Log("index >= values.Length");
                        result = array[array.Length - 1];
                    }
                    else
                    {
                        Debug.Log(string.Format("RETURNING INDEX {0} for {1}", num2, key));
                        result = array[num2];
                    }
                }
                else
                {
                    Debug.Log("NOT PRASED numberPart");
                    result = fallback;
                }
            }
            catch
            {
                Debug.Log("RETURN CATCH");
                result = fallback;
            }
            return result;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002608 File Offset: 0x00000808
        private string GetKeyEnsureLocalization(string key, ModConfigData.KeyType keyType, string locKey = null)
        {
            if (!key.StartsWith("STRING:") || locKey == null)
            {
                return key;
            }
            string value = key.Replace("STRING:", "");
            string text = Plugin.ModAssemblyName + "." + locKey;
            if (keyType == ModConfigData.KeyType.Header)
            {
                text += ".header";
            }
            else if (keyType == ModConfigData.KeyType.Label)
            {
                text += ".label";
            }
            else if (keyType == ModConfigData.KeyType.Tooltip)
            {
                text += ".tooltip";
            }
            else if (keyType == ModConfigData.KeyType.Description)
            {
                text += ".desc";
            }
            if (Localization.HasKey(text))
            {
                return text;
            }
            LocalizationHelper.AddKeyToAllDictionaries(text, value);
            return text;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x000026A0 File Offset: 0x000008A0
        private void CreateConfig()
        {
            if (!File.Exists(this.ConfigPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.ConfigPath));
                File.Create(this.ConfigPath).Close();
            }
        }

        // Token: 0x0600001A RID: 26 RVA: 0x000026D0 File Offset: 0x000008D0
        private void LoadConfig()
        {
            if (!File.Exists(this.ConfigPath))
            {
                this.CreateConfig();
                return;
            }
            foreach (string text in File.ReadAllLines(this.ConfigPath))
            {
                if (!text.StartsWith("#") && !string.IsNullOrWhiteSpace(text))
                {
                    string[] array2 = text.Split(new char[]
                    {
                        '='
                    });
                    if (array2.Length == 2)
                    {
                        string key = array2[0].Trim();
                        string text2 = array2[1].Trim();
                        int num;
                        float num2;
                        bool flag;
                        if (int.TryParse(text2, out num))
                        {
                            this.Settings.Add(key, num);
                        }
                        else if (float.TryParse(text2, out num2))
                        {
                            this.Settings.Add(key, num2);
                        }
                        else if (bool.TryParse(text2, out flag))
                        {
                            this.Settings.Add(key, flag);
                        }
                        else
                        {
                            this.Settings.Add(key, text2);
                        }
                    }
                }
            }
        }

        // Token: 0x0600001B RID: 27 RVA: 0x000027D4 File Offset: 0x000009D4
        private void SaveConfig()
        {
            if (!File.Exists(this.ConfigPath))
            {
                this.CreateConfig();
            }
            File.WriteAllLines(this.ConfigPath, from entry in this.Settings
                                                select string.Format("{0}={1}", entry.Key, entry.Value));
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00002829 File Offset: 0x00000A29
        protected virtual bool OnSave(Dictionary<string, object> newConfig, out string feedbackMessage)
        {
            feedbackMessage = "Saving";
            this.Settings = newConfig;
            this.SaveConfig();
            return true;
        }

        // Token: 0x04000004 RID: 4
        private string ConfigPath;

        // Token: 0x04000005 RID: 5
        private Dictionary<string, object> Settings;

        // Token: 0x04000006 RID: 6
        private List<IConfigValue> ConfigValues;

        // Token: 0x02000015 RID: 21
        public enum KeyType
        {
            // Token: 0x04000034 RID: 52
            Header,
            // Token: 0x04000035 RID: 53
            Label,
            // Token: 0x04000036 RID: 54
            Tooltip,
            // Token: 0x04000037 RID: 55
            Description
        }
    }
}
