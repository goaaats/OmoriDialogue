using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    interface IPayload
    {
        public string RawText { get; set; }
        public string ToHtml();
    }
}
