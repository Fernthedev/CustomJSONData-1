using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomNoteData : NoteData
    {
        private CustomNoteData(float time, int lineIndex, NoteLineLayer noteLineLayer, NoteLineLayer startNoteLineLayer, ColorType colorType, NoteCutDirection cutDirection,
                              float timeToNextColorNote, float timeToPrevColorNote, int flipLineIndex, float flipYSide, float duration, JObject customData)
                       : base(time, lineIndex, noteLineLayer, startNoteLineLayer, colorType, cutDirection, timeToNextColorNote, timeToPrevColorNote, flipLineIndex, flipYSide, duration)
        {
            this.customData = customData;
        }

        public JObject customData { get; private set; }

        public override BeatmapObjectData GetCopy()
        {
            return new CustomNoteData(time, lineIndex, noteLineLayer, startNoteLineLayer, colorType, cutDirection, timeToNextColorNote, timeToPrevColorNote, flipLineIndex, flipYSide, duration, Trees.copy(customData));
        }

        internal static CustomNoteData CreateBombNoteData(float time, int lineIndex, NoteLineLayer noteLineLayer, JObject customData)
        {
            return new CustomNoteData(time, lineIndex, noteLineLayer, noteLineLayer, ColorType.None, NoteCutDirection.None, 0f, 0f, lineIndex, 0f, 0f, customData);
        }

        internal static CustomNoteData CreateBasicNoteData(float time, int lineIndex, NoteLineLayer noteLineLayer, ColorType colorType, NoteCutDirection cutDirection, JObject customData)
        {
            return new CustomNoteData(time, lineIndex, noteLineLayer, noteLineLayer, colorType, cutDirection, 0f, 0f, lineIndex, 0f, 0f, customData);
        }

        internal static CustomNoteData CreateLongNoteData(float time, int lineIndex, NoteLineLayer noteLineLayer, ColorType colorType, NoteCutDirection cutDirection, float duration, JObject customData)
        {
            return new CustomNoteData(time, lineIndex, noteLineLayer, noteLineLayer, colorType, cutDirection, 0f, 0f, lineIndex, 0f, duration, customData);
        }
    }
}
