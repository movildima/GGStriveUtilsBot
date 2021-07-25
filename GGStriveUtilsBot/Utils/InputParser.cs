using System;
using System.Text.RegularExpressions;

namespace GGStriveUtilsBot.Utils
{
    static class InputParser
    {
        // Part of regex that captures both shortened and full names
        static private string charaPattern = String.Join(
            "",
            @"^(?<chara>(",
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
            @"))?\s*"
            );
        // Part of regex that captures either move names or numpad notated moves
        static private string movePattern = String.Join(
            "",
            @"((?<literal>(([a-z]*\s*)*)$)|",
            @"(?<numpad>(([cfj]|(bt))?\.?\d*(\]|\[)?\d?(p|k|s|hs?|d)?(\]|\[)?\d?\s*)*$))"
        );
        static private string charaMovePattern = String.Join("", charaPattern, movePattern);

        static private Regex charaMoveRegex = new Regex(charaMovePattern,
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static private Regex prefixMoveRegex = new Regex(@"^(j|bt)\d{0,6}(p|k|s|hs?|d)|(c|f)s{1,3}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static (string chara, string move, bool isNumpad) parseFrameDataInput(string input) {
            MatchCollection matches = charaMoveRegex.Matches(input);
            if (matches.Count == 0) {
                return ("Unknown", "Unknown", false);
            }
            Match match = matches[0];
            GroupCollection groups = match.Groups;

            // denotes a move's full name ("Gunflame") 
            var literal = groups["literal"].Value.ToString().ToLower();
            // denotes a move in numpad notation ("236P")
            var numpad = groups["numpad"].Value.ToString().ToLower();
            numpad = numpad.Replace("hs", "h");
            
            // 'chara' denotes a character name
            string chara = groups["chara"].Value.ToString().ToLower();
            chara = chara.Length > 1 ? chara : null;
            string move = numpad.Length > literal.Length ? numpad : literal;
            bool isNumpad = numpad.Length > literal.Length;

            // Correct the case where a user inputs j2K instead of j.2K, cS instead of c.S, etc.
            MatchCollection prefixMoveMatches = prefixMoveRegex.Matches(move);
            if (prefixMoveMatches.Count > 0) {
                isNumpad = true;
                if (move.StartsWith("bt")) {
                    move = move.Insert(2, ".");
                } else {
                    move = move.Insert(1, ".");
                }
            }

            // If for some reason a user enters numpad notation without a character,
            // this *CAN* be parsed, but should fail further down the line when fetching moves.
            // (We have no way to know which "5K" in the game they're referring to, for example)

            return (chara, move, isNumpad);
        }
    }
}
