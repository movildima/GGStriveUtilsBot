using System;
using System.Text.RegularExpressions;

namespace GGStriveUtilsBot.Utils
{
    static class InputParser
    {
        // Part of regex that captures both shortened and full names
        static private string charaPattern = String.Join(
            "",
            @"(^(?<chara>",
            @"(sol)(?:\s+badguy)?|",
            @"(ky)(?:\s+kiske)?|",
            @"(may)|",
            @"(faust)|",
            @"(ino)|",
            @"(ram)(?:lethal)?(?:\s+valentine)?|",
            @"(zato)(?:-1)?|",
            @"(nago)(?:ryuki)?|",
            @"(pot)(emkin)?|",
            @"(gio)(vanna)?|",
            @"(millia)(?:\s+rage)?|",
            @"(leo)(?:\s+whitefang)?|",
            @"(chipp)(?:\s+zanuff)?|",
            @"(anji)(?:\s+mito)?|",
            @"(axl)(?:\s+low)?",
            @"))(\s*)"
            );
        // Part of regex that captures either move names or numpad notated moves
        static private string movePattern = @"(?<move>((?<literal>([a-z]*\s*)*)|(?<prefix>[cfj])?(\.)?(\d+)(?<suffix>p|k|s|hs?|d)))$";
        static private string charaMovePattern = String.Join("", charaPattern, movePattern);

        static private Regex charaMoveRegex = new Regex(charaMovePattern,
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static (string chara, string move) parseFrameDataInput(string input) {
            MatchCollection matches = charaMoveRegex.Matches(input);
            if (matches.Count == 0) {
                return ("Unknown", "Unknown");
            }
            Match match = matches[0];
            GroupCollection groups = match.Groups;

            // 'literal' denotes a move's full name ("Gunflame") 
            string literal = groups["prefix"].Value.ToString();
            // 'prefix' denotes close ('c'), far ('f'), or jump ('j')
            string prefix = groups["prefix"].Value.ToString();
            // 'suffix' denotes attack button ('p', 'k', 's', 'h', 'd')
            string suffix = groups["suffix"].Value.ToString();

            // 'chara' denotes a character name
            string chara = groups["chara"].Value.ToString();
            // 'move' denotes an attack, which could be either it's
            // literal name (see above), or it's numpad notation.
            string move = groups["move"].Value.ToString();

            // Replace 'HS' -> 'H' to match dustloop format
            if (!groups["literal"].Success && groups["suffix"].Value.ToString().ToLower() == "hs") {
                move.Replace(suffix, "h");
            }

            return (chara, move);
        }
    }
}
