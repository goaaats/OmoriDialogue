using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class IncreaseTextSizePayload : IPayload
    {
        public string RawText { get; set; }
        public string ToHtml() => string.Empty;
    }
}
