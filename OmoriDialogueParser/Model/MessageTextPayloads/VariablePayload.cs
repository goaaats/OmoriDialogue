using System;
using System.Collections.Generic;
using System.Text;

namespace OmoriDialogueParser.Model.MessageTextPayloads
{
    class VariablePayload : IPayload
    {
        public VariablePayload(int id, string name)
        {
            this.VarId = id;
            this.Name = name;
        }

        public int VarId { get; set; }
        public string Name { get; set; }

        public string RawText { get; set; }
        public string ToHtml() => $"<a href=\"variables.html#v{VarId}\" class=\"speciallink\">VARIABLE({Name})</a>";
    }
}
