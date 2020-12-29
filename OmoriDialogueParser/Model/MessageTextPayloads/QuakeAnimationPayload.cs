using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class QuakeAnimationPayload : IPayload
    {
        public QuakeAnimationPayload(bool enabled)
        {
            this.Enabled = enabled;
        }

        public string RawText { get; set; }
        public bool Enabled { get; set; }

        public string ToHtml() => Enabled ? "<span class=\"quake\">" : "</span>";
    }
}
