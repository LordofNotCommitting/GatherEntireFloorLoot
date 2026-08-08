using System;
using SimpleJSON;

namespace GatherEntireFloorLoot
{
    public class SlotEntry
    {
        public SlotEntry(object value, ValType type)
        {
            this.Value = value;
            this.Type = type;
        }

        public SlotEntry(JSONNode node)
        {
            try
            {
                this.Type = (ValType)Enum.Parse(typeof(ValType), node["Type"]);
                switch (this.Type)
                {
                    case ValType.Bool:
                        this.Value = node["Value"].AsBool;
                        break;
                    case ValType.Int:
                        this.Value = int.Parse(node["Value"]);
                        break;
                    case ValType.Float:
                        this.Value = float.Parse(node["Value"]);
                        break;
                    case ValType.String:
                        this.Value = node["Value"];
                        break;
                }
            }
            catch (Exception arg)
            {
                this.Value = 0;
                this.Type = ValType.Int;
                Plugin.Logger.LogError(string.Format("[SlotEntry] Deserialization error: {0}", arg));
            }
        }

        public void SetValue(object value, ValType type)
        {
            this.Value = value;
            this.Type = type;
        }

        public T GetValue<T>(T fallback = default(T))
        {
            object value = this.Value;
            if (value is T)
            {
                return (T)((object)value);
            }
            return fallback;
        }

        public JSONNode ToJson()
        {
            JSONObject jsonobject = new JSONObject();
            jsonobject["Type"] = this.Type.ToString();
            switch (this.Type)
            {
                case ValType.Bool:
                    jsonobject["Value"] = (bool)this.Value;
                    break;
                case ValType.Int:
                    jsonobject["Value"] = (int)this.Value;
                    break;
                case ValType.Float:
                    jsonobject["Value"] = (float)this.Value;
                    break;
                case ValType.String:
                    jsonobject["Value"] = (string)this.Value;
                    break;
            }
            return jsonobject;
        }

        public object Value;

        public ValType Type;
    }
}
