using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class FontPayload : IPayload
    {
        public string FontName { get; set; }

        public FontPayload(string fontname)
        {
            this.FontName = fontname;
        }

        public string RawText { get; set; }
        public string ToHtml() => string.Empty; // Postprocessed in PayloadList
    }
}
