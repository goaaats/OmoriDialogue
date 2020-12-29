using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OmoriDialogueParser.Model
{
    class ExportMessage
    {
        [JsonProperty("s")] public string Speaker { get; set; }
        [JsonProperty("h")] public string Html { get; set; }

        [JsonProperty("b")] public int? Background { get; set; }
        [JsonProperty("fs")] public string FaceSet { get; set; }
        [JsonProperty("fi")] public int? FaceIndex { get; set; }
    }
}
