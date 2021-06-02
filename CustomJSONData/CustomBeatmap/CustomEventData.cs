using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomEventData
    {
        public string type { get; protected set; }
        public float time { get; protected set; }
        public JObject data { get; protected set; }

        public CustomEventData(float time, string type, JObject data)
        {
            this.time = time;
            this.type = type;
            this.data = data;
        }
    }
}
