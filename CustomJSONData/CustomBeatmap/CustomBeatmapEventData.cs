using System.Collections.Generic;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBeatmapEventData : BeatmapEventData
    {
        public IDictionary<string, object> customData { get; private set; }

        public CustomBeatmapEventData(float time, BeatmapEventType type, int value, IDictionary<string, object> customData) : base(time, type, value)
        {
            this.customData = customData;
        }
    }
}
