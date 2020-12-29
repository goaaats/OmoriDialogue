using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OmoriDialogueParser.Model;
using OmoriDialogueParser.Model.MessageTextPayloads;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace OmoriDialogueParser
{
    class YamlInterpreter
    {
        public IReadOnlyDictionary<int, Message> Messages { get; private set; }

        private YamlInterpreter(){}

        public static YamlInterpreter Parse(TextReader reader)
        {
            var stream = new YamlStream();
            stream.Load(reader);

            if (stream.Documents.Count == 0)
                return null;

            var mapping =
                (YamlMappingNode) stream.Documents[0].RootNode;

            var parsedMessages = new Dictionary<int, Message>();

            foreach (var (index, messageNode) in mapping.Select((x, i) => (i, x.Value)))
            {
                var yaml = messageNode as YamlMappingNode;

                var parsedMsg = new Message
                {
                    Text = ParsePayloads(yaml.FieldOrNull("text")),
                    Faceset = yaml.FieldOrNull("faceset"),
                    Background = yaml.FieldOrNull("background").ToNullableInt(),
                    Faceindex = yaml.FieldOrNull("faceindex").ToNullableInt(),
                    Position = yaml.FieldOrNull("faceindex").ToNullableInt(),
                };

                parsedMessages.Add(index, parsedMsg);
            }

            return new YamlInterpreter
            {
                Messages = parsedMessages
            };
        }

        private static PayloadList ParsePayloads(string text)
        {
            if (text == null)
                throw new ArgumentException("Text cannot be null.", nameof(text));

            var list = new PayloadList();
            var currentPayloadText = string.Empty;

            // Let's iterate through the message and parse out all payloads, one by one.
            for (var i = 0; i < text.Length; i++)
            {
                currentPayloadText += text[i];

                if (TryGetPayload(currentPayloadText, out var parsedPayload))
                {
                    if (!(parsedPayload is TextPayload))
                        list.Add(parsedPayload);
                    else if (!string.IsNullOrEmpty(parsedPayload.RawText))
                        list.Add(parsedPayload);
                    
                    currentPayloadText = string.Empty;
                }

                if (i < text.Length - 1 && text[i + 1] == '\\')
                {
                    list.Add(new TextPayload(currentPayloadText));

                    currentPayloadText = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(currentPayloadText))
                list.Add(new TextPayload(currentPayloadText));

            return list;
        }

        private static bool TryGetPayload(string text, out IPayload payload)
        {
            payload = new TextPayload(text);

            var sinvRegex = Regex.Match(text, @"\\(SINV|sinv)\[(0|1|2)\]");
            if (sinvRegex.Success)
            {
                payload = new SineWaveAnimationPayload(sinvRegex.Groups[2].ToString() == "1" || sinvRegex.Groups[2].ToString() == "2"); // todo: add the other sinv animation
                return true;
            }

            var nRegex = Regex.Match(text, @"\\(N|n)\<(.*)\>");
            if (nRegex.Success)
            {
                payload = new SpeakerPayload(nRegex.Groups[2].ToString());
                return true;
            }

            var fnRegex = Regex.Match(text, @"\\(fn|Fn)\<(.*)\>");
            if (fnRegex.Success)
            {
                payload = new FontPayload(fnRegex.Groups[2].ToString());
                return true;
            }

            var cRegex = Regex.Match(text, @"\\(C|c)\[(\d*)\]");
            if (cRegex.Success)
            {
                payload = new ColorPayload(int.Parse(cRegex.Groups[2].ToString()));
                return true;
            }

            // "Commands" - we don't need to parse these, they're mostly for in-game interactions
            var comRegex = Regex.Match(text, @"\\(Com|com)\[(\d*)\]");
            if (comRegex.Success)
            {
                payload = new TextPayload(string.Empty);
                return true;
            }

            var quakeRegex = Regex.Match(text, @"\\(quake|Quake)\[(0|1)\]");
            if (quakeRegex.Success)
            {
                payload = new QuakeAnimationPayload(quakeRegex.Groups[2].ToString() == "1");
                return true;
            }

            switch (text)
            {
                case @"\!": // WAIT FOR INPUT (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;

                case @"\>": // DISABLE WORD WRAP (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;
                case @"\<": // ENABLE WORD WRAP (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;

                case @"\^": // DO NOT WAIT FOR INPUT (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;

                case @"\|": // WAIT FOR 1s (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;
                case @"\.": // WAIT FOR 1/4s (todo: add some kinda icon)
                    payload = new TextPayload(string.Empty);
                    return true;

                // Escape characters
                case "\\\"":
                    payload = new TextPayload("\"");
                    return true;

                // Party substitution
                case @"\n[1]":
                    payload = new TextPayload("OMORI");
                    return true;
                case @"\n[2]":
                    payload = new TextPayload("AUBREY");
                    return true;
                case @"\n[3]":
                    payload = new TextPayload("KEL");
                    return true;
                case @"\n[4]":
                    payload = new TextPayload("HERO");
                    return true;
                case @"\n[8]":
                    payload = new TextPayload("SUNNY");
                    return true;

                // Text size
                case @"\{":
                    payload = new IncreaseTextSizePayload();
                    return true;
                case @"\}":
                    payload = new DecreaseTextSizePayload();
                    return true;

                // Macros
                case @"\aub":
                    payload = new SpeakerPayload("AUBREY");
                    return true;
                case @"\kel":
                    payload = new SpeakerPayload("KEL");
                    return true;
                case @"\her":
                    payload = new SpeakerPayload("HERO");
                    return true;
                case @"\omo":
                    payload = new SpeakerPayload("OMORI");
                    return true;
                case @"\bas":
                    payload = new SpeakerPayload("BASIL");
                    return true;
                case @"\who":
                    payload = new SpeakerPayload("???");
                    return true;
                case @"\mar":
                    payload = new SpeakerPayload("MARI");
                    return true;
                case @"\min":
                    payload = new SpeakerPayload("MINCY");
                    return true;
                case @"\art":
                    payload = new SpeakerPayload("ARTIST");
                    return true;
                case @"\spxh":
                    payload = new SpeakerPayload("SPACE EX-HUSBAND");
                    return true;
                case @"\mai":
                    payload = new SpeakerPayload("MAILBOX");
                    return true;
                case @"\swh":
                    payload = new SpeakerPayload("SWEETHEART");
                    return true;
                case @"\kim":
                    payload = new SpeakerPayload("KIM");
                    return true;
                case @"\cha":
                    payload = new SpeakerPayload("CHARLIE");
                    return true;
                case @"\ang":
                    payload = new SpeakerPayload("ANGEL");
                    return true;
                case @"\mav":
                    payload = new SpeakerPayload("THE MAVERICK");
                    return true;
                case @"\van":
                    payload = new SpeakerPayload("VANCE");
                    return true;
                case @"\spg":
                    payload = new SpeakerPayload("SPACE PIRATE GUY");
                    return true;
                case @"\spd":
                    payload = new SpeakerPayload("SPACE PIRATE DUDE");
                    return true;
                case @"\spb":
                    payload = new SpeakerPayload("SPACE PIRATE BUDDY");
                    return true;
                case @"\wis":
                    payload = new SpeakerPayload("WISE ROCK");
                    return true;
                case @"\ber":
                    payload = new SpeakerPayload("BERLY");
                    return true;
                /* TODO: This is a duplicate macro... bug in the game?
                case @"\van":
                    payload = new SpeakerPayload("VAN");
                    return true;
                */
                case @"\nos":
                    payload = new SpeakerPayload("NOSE");
                    return true;
                case @"\bun":
                    payload = new SpeakerPayload("BUN");
                    return true;
                case @"\lad":
                    payload = new SpeakerPayload("MIKAL");
                    return true;
                case @"\dai":
                    payload = new SpeakerPayload("DAISY");
                    return true;
                case @"\neb":
                    payload = new SpeakerPayload("NEB");
                    return true;
                case @"\hap":
                    payload = new SpeakerPayload("HAPPY");
                    return true;
                case @"\eye":
                    payload = new SpeakerPayload("EYEBROWS");
                    return true;
                case @"\ban":
                    payload = new SpeakerPayload("BANGS");
                    return true;
                case @"\shaw":
                    payload = new SpeakerPayload("SHAWN");
                    return true;
                case @"\ren":
                    payload = new SpeakerPayload("REN");
                    return true;
                case @"\char":
                    payload = new SpeakerPayload("CHARLENE");
                    return true;
                case @"\wee":
                    payload = new SpeakerPayload("WEEPING WILLOW");
                    return true;
                case @"\hum":
                    payload = new SpeakerPayload("HUMPHREY");
                    return true;
                case @"\gra":
                    payload = new SpeakerPayload("GRANDMA");
                    return true;
                case @"\che":
                    payload = new SpeakerPayload("CHEERS");
                    return true;
                case @"\sna":
                    payload = new SpeakerPayload("SNALEY");
                    return true;
                case @"\swe":
                    payload = new SpeakerPayload("SWEETHEART");
                    return true;
                case @"\ems":
                    payload = new SpeakerPayload("EMS");
                    return true;
                case @"\ash":
                    payload = new SpeakerPayload("ASH");
                    return true;
                case @"\plu":
                    payload = new SpeakerPayload("PLUTO");
                    return true;
                case @"\due":
                    payload = new SpeakerPayload("DUE");
                    return true;
                case @"\cru":
                    payload = new SpeakerPayload("CRUE");
                    return true;
                case @"\ros":
                    payload = new SpeakerPayload("ROSA");
                    return true;
                case @"\kit":
                    payload = new SpeakerPayload("KITE KID");
                    return true;
                case @"\sca":
                    payload = new SpeakerPayload("SCARECROW");
                    return true;
                case @"\tvg":
                    payload = new SpeakerPayload("TVGIRL");
                    return true;
                case @"\sha":
                    payload = new SpeakerPayload("SHADY MOLE");
                    return true;
                case @"\may":
                    payload = new SpeakerPayload("MAYOR MOLE");
                    return true;
                case @"\sle":
                    payload = new SpeakerPayload("SLEEPY MOLE");
                    return true;
                case @"\spo":
                    payload = new SpeakerPayload("SPORTY MOLE");
                    return true;
                /* TODO: also duplicate?
                case @"\che":
                    payload = new SpeakerPayload("CHEF MOLE");
                    return true;
                */
                case @"\spr":
                    payload = new SpeakerPayload("SPROUT MOLE");
                    return true; 
                /* todo: also duplicate?
                case @"\ban":
                    payload = new SpeakerPayload("BANDITO MOLE");
                    return true;
                */
                case @"\toa":
                    payload = new SpeakerPayload("TOASTY");
                    return true;
                /*
                case @"\bun":
                    payload = new SpeakerPayload("BUN BUNNY");
                    return true;
                */
                case @"\ma1":
                    payload = new SpeakerPayload("MAFIALLIGATOR 1");
                    return true;
                case @"\ma2":
                    payload = new SpeakerPayload("MAFIALLIGATOR 2");
                    return true;
                case @"\spe":
                    payload = new SpeakerPayload("SPELLING BEE");
                    return true;
                case @"\hot":
                    payload = new SpeakerPayload("HOTDOG");
                    return true;
                case @"\bud":
                    payload = new SpeakerPayload("BUDGIRL");
                    return true;
                case @"\tom":
                    payload = new SpeakerPayload("TOMATO GIRL");
                    return true;
                case @"\lea":
                    payload = new SpeakerPayload("LEAFY");
                    return true;
                case @"\ora":
                    payload = new SpeakerPayload("ORANGE JOE");
                    return true;
                case @"pro":
                    payload = new SpeakerPayload("PROPELLER GHOST");
                    return true;
                case @"\str":
                    payload = new SpeakerPayload("STRAW HAT GHOST");
                    return true;
                case @"\top":
                    payload = new SpeakerPayload("TOP HAT GHOST");
                    return true;
                case @"\po":
                    payload = new SpeakerPayload("PO");
                    return true;
                case @"\lar":
                    payload = new SpeakerPayload("LARAMIE");
                    return true;
                case @"\fer":
                    payload = new SpeakerPayload("FERRIS");
                    return true;
                case @"\gum":
                    payload = new SpeakerPayload("GUMBO");
                    return true;
                case @"\gib":
                    payload = new SpeakerPayload("GIBS");
                    return true;
                case @"\cre":
                    payload = new SpeakerPayload("CREEPY CAT");
                    return true;
                case @"\duw":
                    payload = new SpeakerPayload("DUCK WIFE");
                    return true;
                case @"\duj":
                    payload = new SpeakerPayload("DUCK JR.");
                    return true;
                case @"\duc":
                    payload = new SpeakerPayload("DUCK");
                    return true;
                case @"\pes":
                    payload = new SpeakerPayload("PESSI");
                    return true;
                case @"\smo":
                    payload = new SpeakerPayload("SMOL");
                    return true;
                case @"\gen":
                    payload = new SpeakerPayload("GENKI");
                    return true;
                case @"\LUE":
                    payload = new SpeakerPayload("LUE");
                    return true;
                case @"\pol":
                    payload = new SpeakerPayload("POLAR BEAR");
                    return true;
                case @"\sou":
                    payload = new SpeakerPayload("SOUS-CHEF MOLE");
                    return true;
                case @"\tea":
                    payload = new SpeakerPayload("TEACHER MOLE");
                    return true;
                case @"\st1":
                    payload = new SpeakerPayload("STUDENT MOLE 1");
                    return true;
                case @"\st2":
                    payload = new SpeakerPayload("STUDENT MOLE 2");
                    return true;
                case @"\st3":
                    payload = new SpeakerPayload("STUDENT MOLE 3");
                    return true;
                case @"\dun":
                    payload = new SpeakerPayload("DUNCE MOLE");
                    return true;
                case @"\lau":
                    payload = new SpeakerPayload("LAUNDRY MOLE");
                    return true;
                case @"\squ":
                    payload = new SpeakerPayload("SQUARE MOLE");
                    return true;
                case @"\dm1":
                    payload = new SpeakerPayload("DINING MOLE 1");
                    return true;
                case @"\dm2":
                    payload = new SpeakerPayload("DINING MOLE 2");
                    return true;
                case @"\dm3":
                    payload = new SpeakerPayload("DINING MOLE 3");
                    return true;
                case @"\mm1":
                    payload = new SpeakerPayload("MISC MOLE 1");
                    return true;
                case @"\mm2":
                    payload = new SpeakerPayload("MISC MOLE 2");
                    return true;
                case @"\joc":
                    payload = new SpeakerPayload("JOCK JAMS");
                    return true;
                case @"\sp1":
                    payload = new SpeakerPayload("SPACE CREW 1");
                    return true;
                case @"\sp2":
                    payload = new SpeakerPayload("SPACE CREW 2");
                    return true;
                case @"\sp3":
                    payload = new SpeakerPayload("SPACE CREW 3");
                    return true;
                case @"\ear":
                    payload = new SpeakerPayload("EARTH");
                    return true;
                case @"\sbf":
                    payload = new SpeakerPayload("SPACE BOYFRIEND");
                    return true;
                case @"\sxbf":
                    payload = new SpeakerPayload("SPACE EX-BOYFRIEND");
                    return true;
                case @"\cap":
                    payload = new SpeakerPayload("CAPT. SPACEBOY");
                    return true;
                case @"\shb":
                    payload = new SpeakerPayload("SPACE HUSBAND");
                    return true;
                case @"\sxhb":
                    payload = new SpeakerPayload("SPACE EX-HUSBAND");
                    return true;
                case @"\kev":
                    payload = new SpeakerPayload("KEVIN");
                    return true;

                default:  
                    return false;
            }
        }
    }
}
