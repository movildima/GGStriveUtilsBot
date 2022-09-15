using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot.Utils
{
    static class InputParser
    {
        // Part of regex that captures both shortened and full names
        static private string charaPattern = String.Join(
            "",
            @"^(?<chara>(",
            @"(?<Sol>sol)(?:\s+badguy)?|",
            @"(?<Ky>ky)(?:\s+kiske)?|",
            @"(?<May>may)|",
            @"(?<Faust>faust)|",
            @"(?<Ino>i-?no)|",
            @"(?<Ram>ram)(?:lethal)?(?:\s+valentine)?|",
            @"(?<Zato>zato)(?:-1)?|",
            @"(?<Nago>nago)(?:riyuki)?|",
            @"(?<Pot>pot)(?:emkin)?|",
            @"(?<Gio>gio)(?:vanna)?|",
            @"(?<Millia>mill?ia)(?:\s+rage)?|",
            @"(?<Leo>leo)(?:\s+whitefang)?|",
            @"(?<Chipp>chipp)(?:\s+zanuff)?|",
            @"(?<Anji>anji)(?:\s+mito)?|",
            @"(?<Axl>axl)(?:\s+low)?|",
            @"(?<Goldlewis>gold)(?:lewis)?(?:\s+dickinson)?|",
            @"(?<Jacko>jack-?o)|",
            @"(?<Baiken>baiken)|",
            @"(?<Testament>test)(?:ament)?|",
            @"(?<HappyChaos>((happy(?:\s+chaos)?)|(hc)|(chaos)|(fatherless)))|",
            @"(?<Bridget>bridget)",
            @"))?\s*"
            );
        // Part of regex that captures either move names or numpad notated moves
        static private string movePattern = String.Join(
            "",
            @"((?<numpad>((([cfj]|(bt)|\-)?\.?\d*(\]|\[)?\d?(p|k|s|hs?|d)?(\]|\[)?\d?\s*)|", // general numpad notation
            @"(\[((s\/h)|(h\/s))\]\s*((s\/h)|(h\/s))))*)|",                               // (exception for Leo's janky guard attack)
            @"(?<literal>(([a-z\.\'\?]*\s*)*)))",                                         // literal move names (e.g. "stun edge")
            @"(?<level>((Level\s(1|2|3|(br)){1})|([2468]{3}))?$)"                         // 'leveled' moves with multiple entries
        );
        static private string charaMovePattern = String.Join("", charaPattern, movePattern);

        static private Regex charaMoveRegex = new Regex(charaMovePattern,
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static private Regex prefixMoveRegex = new Regex(@"^(j|bt)\d{0,6}(p|k|s|hs?|d)|(c|f)s{1,3}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Returns the Character enum, move name string, move level string, and a bool indicating numpad format
        public static (Character? character, string move, string level, bool isNumpad) parseFrameDataInput(string input)
        {
            MatchCollection matches = charaMoveRegex.Matches(input);
            if (matches.Count == 0)
            {
#if DEBUG
                Console.WriteLine("\nUnable to parse.");
#endif
                return (null, "Unknown", "", false);
            }

            Match match = matches[0];
            GroupCollection groups = match.Groups;

            Character? character = null;
            foreach (var v in Enum.GetValues(typeof(Character)))
            {
                if (groups[v.ToString()].Length > 0)
                {
                    character = (Character)Enum.Parse(typeof(Character), v.ToString());
                }
            }

            // denotes a move's full name ("Gunflame") 
            var literal = groups["literal"].Value.ToString().ToLower();
            // denotes a move in numpad notation ("236P")
            var numpad = groups["numpad"].Value.ToString().ToLower();
            // denotes the "level" modifier for certain moves such as,
            // Goldlewis 214S Levels 1/2/3, Nago 5H Levels 1/2/3
            // Also used to denote Behemoth Typhoon shorthand half-circle notation
            // For example, behemoth typhoon "426" referring to the 41236 variation
            var level = groups["level"].Value.ToString().ToLower();

            string move = numpad.Length > literal.Length ? numpad : literal;
            bool isNumpad = numpad.Length > literal.Length;

            // Correct the case where a user inputs j2K instead of j.2K, cS instead of c.S, etc.
            MatchCollection prefixMoveMatches = prefixMoveRegex.Matches(move);
            if (prefixMoveMatches.Count > 0)
            {
                isNumpad = true;
                if (move.StartsWith("bt"))
                {
                    move = move.Insert(2, ".");
                }
                else
                {
                    move = move.Insert(1, ".");
                }
            }

            if (isNumpad) {
                move = move.Replace("hs", "h");
            }

            move = move.Trim();
            level = level.Trim();

            // If for some reason a user enters numpad notation without a character,
            // this *CAN* be parsed, but should fail further down the line when fetching moves.
            // (We have no way to know which "5K" in the game they're referring to, for example)

#if DEBUG
            Console.WriteLine("\nCharacter: " + character.ToString());
            Console.WriteLine("Move: " + move);
            Console.WriteLine("Move Level: " + level);
            Console.WriteLine("IsNumpad: " + isNumpad.ToString());
#endif

            return (character, move, level, isNumpad);
        }
    }
}
