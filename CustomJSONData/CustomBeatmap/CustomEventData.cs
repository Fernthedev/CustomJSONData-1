using System.Collections.Generic;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomEventData
    {
        public string type { get; protected set; }
        public float time { get; protected set; }
        public IDictionary<string, object> data { get; protected set; }

        public CustomEventData(float time, string type, IDictionary<string, object> data)
        {
            this.time = time;
            this.type = type;
            this.data = data;
        }
    }
}
