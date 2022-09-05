using Newtonsoft.Json;

namespace SammBotNET.RestDefinitions
{
    public class Rule34SearchParams
    {
        public int limit { get; set; }
        public int pid { get; set; }
        public string tags { get; set; }
        public uint cid { get; set; }
        public int id { get; set; }
        public int json { get; set; }
    }

    public class Rule34Post
    {
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }
        [JsonProperty("sample_url")]
        public string SampleUrl { get; set; }
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
        [JsonProperty("directory")]
        public int Directory { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("change")]
        public long Change { get; set; }
        [JsonProperty("owner")]
        public string Owner { get; set; }
        [JsonProperty("parent_id")]
        public long OwnerId { get; set; }
        [JsonProperty("rating")]
        public string Rating { get; set; }
        [JsonProperty("sample")]
        public int Sample { get; set; }
        [JsonProperty("sample_height")]
        public int SampleHeight { get; set; }
        [JsonProperty("sample_width")]
        public int SampleWidth { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("tags")]
        public string Tags { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
