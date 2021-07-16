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
            @")?)(\s*)"
            );
        // Part of regex that captures either move names or numpad notated moves
        static private string movePattern = String.Join(
            "",
            @"(?<move>((?<literal>([a-z]*\s*)*)|",
            @"(?<prefix>[cfj])?(\.)?(\d*)(\]|\[)?(?<suffix>p|k|s|hs?|d)(\]|\[)?)*)$"
        );
        static private string charaMovePattern = String.Join("", charaPattern, movePattern);

        static private Regex charaMoveRegex = new Regex(charaMovePattern,
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static (string chara, string move, bool isNumpad) parseFrameDataInput(string input) {
            MatchCollection matches = charaMoveRegex.Matches(input);
            if (matches.Count == 0) {
                return ("Unknown", "Unknown", false);
            }
            Match match = matches[0];
            GroupCollection groups = match.Groups;

            // denotes a move's full name ("Gunflame") 
            var literal = groups["literal"];
            // denotes close ('c'), far ('f'), or jump ('j')
            // NOTE: May contain multiple individual captures
            _ = groups["prefix"];
            // denotes attack button ('p', 'k', 's', 'h', 'd')
            // NOTE: May contain multiple individual captures
            _ = groups["suffix"];
            
            // 'chara' denotes a character name
            string chara = groups["chara"].Value.ToString();
            // 'move' denotes an attack, which could be either it's
            // literal name (see above), or it's numpad notation.
            string move = groups["move"].Value.ToString().ToLower();

            bool isNumpad = false;

            // Replace 'HS' -> 'H' to match dustloop format
            if (literal.Length < 1) {
                isNumpad = true;
                move = move.Replace("hs", "h");
            }

            // Character name & move are both specified
            if (chara.Length > 1) {
                return (chara, move, isNumpad);
            }

            // Move is specified without character name
            return (null, move, isNumpad);
        }
    }
}
