﻿namespace CustomJSONData.CustomLevelInfo
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using static CustomJSONData.Trees;

    public class CustomLevelInfoSaveData : StandardLevelInfoSaveData
    {
        protected IDictionary<string, object> _customData;

        public IDictionary<string, object> customData => _customData;

        public Dictionary<string, IDictionary<string, object>> beatmapCustomDatasByFilename { get; protected set; }

        public CustomLevelInfoSaveData(string songName, string songSubName, string songAuthorName, string levelAuthorName, float beatsPerMinute, float songTimeOffset,
                                       float shuffle, float shufflePeriod, float previewStartTime, float previewDuration, string songFilename, string coverImageFilename,
                                       string environmentName, string allDirectionsEnvironmentName, DifficultyBeatmapSet[] difficultyBeatmapSets, IDictionary<string, object> customData, Dictionary<string, IDictionary<string, object>> beatmapCustomDatasByFilename)
                                : base(songName, songSubName, songAuthorName, levelAuthorName, beatsPerMinute, songTimeOffset, shuffle, shufflePeriod, previewStartTime,
                                        previewDuration, songFilename, coverImageFilename, environmentName, allDirectionsEnvironmentName, difficultyBeatmapSets)
        {
            _customData = customData;
            this.beatmapCustomDatasByFilename = beatmapCustomDatasByFilename;
        }

        public static StandardLevelInfoSaveData DeserializeFromJSONString(string stringData, StandardLevelInfoSaveData standardSaveData)
        {
            Dictionary<string, IDictionary<string, object>> beatmapsByFilename = new Dictionary<string, IDictionary<string, object>>();
            DifficultyCustomDatas customDatas = JsonConvert.DeserializeObject<DifficultyCustomDatas>(stringData);
            DifficultyBeatmapSet[] customBeatmapSets = new DifficultyBeatmapSet[standardSaveData.difficultyBeatmapSets.Length];
            for (int i = 0; i < standardSaveData.difficultyBeatmapSets.Length; i++)
            {
                DifficultyBeatmapSet standardBeatmapSet = standardSaveData.difficultyBeatmapSets[i];
                DifficultyBeatmap[] customBeatmaps = new DifficultyBeatmap[standardBeatmapSet.difficultyBeatmaps.Length];
                for (int j = 0; j < standardBeatmapSet.difficultyBeatmaps.Length; j++)
                {
                    StandardLevelInfoSaveData.DifficultyBeatmap standardBeatmap = standardBeatmapSet.difficultyBeatmaps[j];
                    DifficultyBeatmap customBeatmap = new DifficultyBeatmap(standardBeatmap.difficulty, standardBeatmap.difficultyRank, standardBeatmap.beatmapFilename, standardBeatmap.noteJumpMovementSpeed,
                                                                            standardBeatmap.noteJumpStartBeatOffset, customDatas._difficultyBeatmapSets[i]._difficultyBeatmaps[j]._customData ?? Tree());
                    customBeatmaps[j] = customBeatmap;
                    beatmapsByFilename[customBeatmap.beatmapFilename] = customBeatmap.customData;
                }
                customBeatmapSets[i] = new DifficultyBeatmapSet(standardBeatmapSet.beatmapCharacteristicName, customBeatmaps);
            }
            return new CustomLevelInfoSaveData(standardSaveData.songName, standardSaveData.songSubName, standardSaveData.songAuthorName, standardSaveData.levelAuthorName,
                                                                         standardSaveData.beatsPerMinute, standardSaveData.songTimeOffset, standardSaveData.shuffle, standardSaveData.shufflePeriod,
                                                                         standardSaveData.previewStartTime, standardSaveData.previewDuration, standardSaveData.songFilename, standardSaveData.coverImageFilename,
                                                                         standardSaveData.environmentName, standardSaveData.allDirectionsEnvironmentName, customBeatmapSets, customDatas._customData ?? Tree(), beatmapsByFilename);
        }

        public new class DifficultyBeatmap : StandardLevelInfoSaveData.DifficultyBeatmap
        {
            protected IDictionary<string, object> _customData;

            public IDictionary<string, object> customData => _customData;

            public DifficultyBeatmap(string difficultyName, int difficultyRank, string beatmapFilename, float noteJumpMovementSpeed, float noteJumpStartBeatOffset, IDictionary<string, object> customData)
                              : base(difficultyName, difficultyRank, beatmapFilename, noteJumpMovementSpeed, noteJumpStartBeatOffset)
            {
                _customData = customData;
            }
        }

        [Serializable]
        private class DifficultyCustomDatas
        {
#pragma warning disable 0649

            [JsonProperty]
            public IDictionary<string, object> _customData;

            [JsonProperty]
            public List<DifficultyBeatmapSet> _difficultyBeatmapSets;

            [Serializable]
            public class DifficultyBeatmapSet
            {
                [JsonProperty]
                public List<DifficultyBeatmap> _difficultyBeatmaps;
            }

            [Serializable]
            public class DifficultyBeatmap
            {
                [JsonProperty]
                public IDictionary<string, object> _customData;
            }

#pragma warning restore 0649
        }
    }
}
