using System;
using System.Collections.Generic;
using SimpleJSON;

namespace GatherEntireFloorLoot
{
    public class SlotData
    {
        public SlotData(JSONNode node = null)
        {
            this.slotData = new Dictionary<string, SlotEntry>();
            if (node != null)
            {
                foreach (string text in node.Keys)
                {
                    this.slotData[text] = new SlotEntry(node[text]);
                }
            }
        }

        public void SetValue<T>(string key, T value)
        {
            ValType valType;
            if (value is bool)
            {
                valType = ValType.Bool;
            }
            else if (value is int)
            {
                valType = ValType.Int;
            }
            else if (value is float)
            {
                valType = ValType.Float;
            }
            else if (value is string)
            {
                valType = ValType.String;
            }
            else
            {
                valType = ValType.Unsupported;
            }
            if (valType != ValType.Unsupported)
            {
                this.slotData[key] = new SlotEntry(value, valType);
                return;
            }
            Plugin.Logger.LogError("[SlotEntry] Unsupported type for SlotData is being used for key:" + key);
            this.slotData[key] = new SlotEntry(0, ValType.Int);
        }

        public T GetValue<T>(string key, T fallback = default(T))
        {
            SlotEntry slotEntry;
            if (this.slotData.TryGetValue(key, out slotEntry))
            {
                return slotEntry.GetValue<T>(fallback);
            }
            Plugin.Logger.LogWarning("Unable to get value for key:" + key + " | Data not found.");
            return fallback;
        }

        public bool Contains(string key)
        {
            return this.slotData.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.slotData.Remove(key);
        }

        public JSONNode ToJson()
        {
            JSONObject jsonobject = new JSONObject();
            foreach (KeyValuePair<string, SlotEntry> keyValuePair in this.slotData)
            {
                jsonobject[keyValuePair.Key] = keyValuePair.Value.ToJson();
            }
            return jsonobject;
        }

        private Dictionary<string, SlotEntry> slotData;
    }
}
