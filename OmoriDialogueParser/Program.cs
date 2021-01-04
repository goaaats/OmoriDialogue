using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OmoriDialogueParser.Model;
using OmoriDialogueParser.Model.Data;
using OmoriDialogueParser.Model.MessageTextPayloads;

namespace OmoriDialogueParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide path to extracted language files.");
                return;
            }

            var langDir = new DirectoryInfo(args[0]);

            var system = JsonConvert.DeserializeObject<Model.System>(File.ReadAllText(Path.Combine("data", "System.json")));

            var fileParsedDict = new Dictionary<string, IEnumerable<ExportMessage>>();

            foreach (var file in langDir.EnumerateFiles("*.yaml"))
            {
                // Skip the monster book
                if (file.Name == "Bestiary.yaml")
                    continue;

                // Skip the database
                if (file.Name == "Database.yaml")
                    continue;

                // Skip menus
                if (file.Name == "menus.yaml")
                    continue;

                // Skip System
                if (file.Name == "System.yaml")
                    continue;

                Console.WriteLine($"-> {file.Name}");
                var textReader = file.OpenText();

                try
                {
                    var languageFile = YamlInterpreter.Parse(textReader, system);

                    if (languageFile == null)
                    {
                        Console.WriteLine($"   -> {file.Name} was empty!");
                        continue;
                    }

                    var messages = languageFile.Messages.Select(languageFileMessage => new ExportMessage
                    {
                        Html = languageFileMessage.Value.Text.ToHtml(),
                        Speaker = languageFileMessage.Value.Text.GetSpeaker(),
                        Background = languageFileMessage.Value.Background,
                        FaceIndex = languageFileMessage.Value.Faceindex,
                        FaceSet = languageFileMessage.Value.Faceset
                    }).ToList();

                    fileParsedDict.Add(Path.GetFileNameWithoutExtension(file.Name), messages);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("  -> YamlInterpreter ERROR:\n" + ex);
                }
                finally
                {
                    textReader.Close();
                }
            }

            File.WriteAllText(Path.Combine("html", "text.js"), $"var t = {JsonConvert.SerializeObject(fileParsedDict)}");

            // Create another version of the object, sorted by character name
            var byCharaDict = new Dictionary<string, Dictionary<string, List<ExportMessage>>>();

            foreach (var parsed in fileParsedDict)
            {
                foreach (var message in parsed.Value)
                {
                    if (string.IsNullOrEmpty(message.Speaker))
                        continue;

                    // We want to filter all of the "time" messages
                    if (Regex.IsMatch(message.Speaker, "[0-9].*"))
                        continue;

                    if (!byCharaDict.ContainsKey(message.Speaker))
                        byCharaDict.Add(message.Speaker, new Dictionary<string, List<ExportMessage>>());

                    if (!byCharaDict[message.Speaker].ContainsKey(parsed.Key))
                        byCharaDict[message.Speaker].Add(parsed.Key, new List<ExportMessage>());

                    byCharaDict[message.Speaker][parsed.Key].Add(message);
                }
            }

            File.WriteAllText(Path.Combine("html", "text_bychara.js"), $"var t = {JsonConvert.SerializeObject(byCharaDict)}");

            // Make variable page
            var variables = string.Empty;
            for (var i = 0; i < system.variables.Count; i++)
            {
                variables +=
                    $"\n<tr>\r\n            <td><a id=\"v{i}\" href=\"#v{i}\">{i}</a></td>\r\n            <td>{system.variables[i]}</td>\r\n        </tr>";
            }

            var varTemplate = File.ReadAllText(Path.Combine("template", "variables.htmt"));
            File.WriteAllText(Path.Combine("html", "variables.html"), string.Format(varTemplate, variables));

            var switches = string.Empty;
            for (var i = 0; i < system.switches.Count; i++)
            {
                switches +=
                    $"\n<tr>\r\n            <td><a id=\"v{i}\" href=\"#v{i}\">{i}</a></td>\r\n            <td>{system.switches[i]}</td>\r\n        </tr>";
            }

            var switchesTemplate = File.ReadAllText(Path.Combine("template", "switches.htmt"));
            File.WriteAllText(Path.Combine("html", "switches.html"), string.Format(switchesTemplate, switches));

            ExportMaps();
        }

        private static void ExportMaps()
        {
            Directory.CreateDirectory(Path.Combine("html", "map", "doc"));

            var mapInfos =
                JsonConvert.DeserializeObject<List<MapEntry>>(File.ReadAllText(Path.Combine("data", "MapInfos.json")))
                    .Where(x => x != null).OrderBy(x => x.Order).ToArray();

            var maps = string.Empty;
            for (var i = 0; i < mapInfos.Length; i++)
            {
                var id = mapInfos[i].Id;

                var mapLink = $"<a href=\"map.html#{id}\">{mapInfos[i].Name}</a>";
                if (!File.Exists(Path.Combine("html", "map", "img", $"map{id}.png")))
                    mapLink = mapInfos[i].Name;

                maps +=
                    $"\n<tr>\r\n            <td><a id=\"m{id}\" href=\"#m{id}\">{id}</a></td>\r\n            <td>{mapLink}</td>\r\n        </tr>";

                var eMap = new ExportMap();
                eMap.Name = mapInfos[i].Name;
                var realDataPath = Path.Combine("data", $"Map{id:D3}.json");
                if (File.Exists(realDataPath))
                    eMap.DataMap = JsonConvert.DeserializeObject(File.ReadAllText(realDataPath));
                File.WriteAllText(Path.Combine("html", "map", "doc", $"map{id}.json"), JsonConvert.SerializeObject(eMap));
            }

            var mapsTemplate = File.ReadAllText(Path.Combine("template", "maps.htmt"));
            File.WriteAllText(Path.Combine("html", "maps.html"), string.Format(mapsTemplate, maps));
        }
    }
}
