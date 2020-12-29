using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace OmoriDialogueParser
{
    static class Util
    {
        public static string FieldOrNull(this YamlMappingNode node, string key)
        {
            if (node.Children.Any(x => x.Key.ToString() == key))
                return node[key].ToString();

            return null;
        }

        public static int? ToNullableInt(this string s)
        {
            if (int.TryParse(s, out var i)) return i;
            return null;
        }
    }
}
