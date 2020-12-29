using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class TextPayload : IPayload
    {
        private readonly bool htmlEncode;

        public TextPayload(string text, bool htmlEncode = true)
        {
            this.htmlEncode = htmlEncode;
            RawText = text;
        }

        public string RawText { get; set; }
        public string ToHtml()
        {
            var text = RawText;

            if (this.htmlEncode)
                text = HttpUtility.HtmlEncode(this);

            return text.Replace("&lt;br&gt;", "<br>");
        }

        public override string ToString() => RawText;
    }
}
