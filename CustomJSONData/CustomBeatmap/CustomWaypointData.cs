using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomWaypointData : WaypointData
    {
        public JObject customData { get; private set; }

        public CustomWaypointData(float time, int lineIndex, NoteLineLayer noteLineLayer, OffsetDirection offsetDirection, JObject customData) : base(time, lineIndex, noteLineLayer, offsetDirection)
        {
            this.customData = customData;
        }

        public override BeatmapObjectData GetCopy()
        {
            return new CustomWaypointData(time, lineIndex, noteLineLayer, offsetDirection, Trees.copy(customData));
        }
    }
}
