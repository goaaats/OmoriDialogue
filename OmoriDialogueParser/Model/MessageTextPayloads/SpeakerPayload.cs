using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class SpeakerPayload : IPayload
    {
        public SpeakerPayload(string name)
        {
            SpeakerName = name;
        }

        public string SpeakerName { get; set; }
        public string RawText { get; set; }
        public string ToHtml() => string.Empty;

        public override string ToString() => SpeakerName;
    }
}
