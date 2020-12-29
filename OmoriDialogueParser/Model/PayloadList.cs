using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmoriDialogueParser.Model.MessageTextPayloads;

namespace OmoriDialogueParser.Model
{
    class PayloadList : List<IPayload>
    {
        public string ToHtml()
        {
            // Text size works in a stack-based fashion - we need to take care of this here.

            var atSizeStack = 4;
            for (var i = 0; i < this.Count; i++)
            {
                var thisPayload = this[i];

                if (thisPayload is IncreaseTextSizePayload)
                {
                    if (atSizeStack >= 4)
                    {
                        atSizeStack++;
                        this[i] = new TextPayload($"<span class=\"s{atSizeStack}\">", false);
                    }
                    else
                    {
                        atSizeStack++;
                        this[i] = new TextPayload("</span>", false);
                    }
                }

                if (thisPayload is DecreaseTextSizePayload)
                {
                    if (atSizeStack <= 4)
                    {
                        atSizeStack--;
                        this[i] = new TextPayload($"<span class=\"s{atSizeStack}\">", false);
                    }
                    else
                    {
                        atSizeStack--;
                        this[i] = new TextPayload("</span>", false);
                    }
                }
            }

            if (atSizeStack < 4)
            {
                while (atSizeStack != 4)
                {
                    this.Add(new TextPayload("</span>", false));
                    atSizeStack++;
                }
            }

            if (atSizeStack > 4)
            {
                while (atSizeStack != 4)
                {
                    this.Add(new TextPayload("</span>", false));
                    atSizeStack--;
                }
            }

            
            //Same thing for fonts

            var atFontStack = 0;
            for (var i = 0; i < this.Count; i++)
            {
                var thisPayload = this[i];

                if (thisPayload is FontPayload fontPayload)
                {
                    var text = string.Empty;
                    if (atFontStack > 0)
                    {
                        text += "</span>";
                        atFontStack--;
                    }

                    atFontStack++;
                    this[i] = new TextPayload(text + $"<span class=\"f{fontPayload.FontName}\">", false);
                }
            }

            if (atFontStack > 0)
                this.Add(new TextPayload("</span>", false));

            return this.Aggregate(string.Empty, (current, payload) => current + payload.ToHtml());
        }

        public string GetSpeaker()
        {
            if (this.FirstOrDefault(x => x is SpeakerPayload) is SpeakerPayload speaker)
                return speaker.SpeakerName;

            return null;
        }
    }
}
