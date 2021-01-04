using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OmoriDialogueParser.Model.Data
{
    class MapEntry
    {
        [JsonProperty("id")]
        public int Id { get; set; } 

        [JsonProperty("expanded")]
        public bool Expanded { get; set; } 

        [JsonProperty("name")]
        public string Name { get; set; } 

        [JsonProperty("order")]
        public int Order { get; set; } 

        [JsonProperty("parentId")]
        public int ParentId { get; set; } 

        [JsonProperty("scrollX")]
        public double ScrollX { get; set; } 

        [JsonProperty("scrollY")]
        public double ScrollY { get; set; } 

        [JsonProperty("type")]
        public string Type { get; set; } 

        [JsonProperty("line")]
        public object Line { get; set; } 
    }
}
