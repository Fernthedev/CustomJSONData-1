using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBeatmapEventData : BeatmapEventData
    {
        public JObject customData { get; private set; }

        public CustomBeatmapEventData(float time, BeatmapEventType type, int value, JObject customData) : base(time, type, value)
        {
            this.customData = customData;
        }
    }
}
