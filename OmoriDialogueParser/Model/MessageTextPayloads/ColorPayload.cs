using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class ColorPayload : IPayload
    {
        public ColorPayload(int colorKey)
        {
            this.ColorKey = colorKey;
        }

        public string RawText { get; set; }
        public int ColorKey { get; set; }

        public string ToHtml() => ColorKey != 0 ? $"<span class=\"c{ColorKey}\">" : "</span>";
    }
}
