using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Linq;
using Fastenshtein;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot.Utils
{
    static class DustloopDataFetcher
    {
        static private string mainQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=MoveData_GGST&&fields=chara%2C+input%2C+name%2C+images%2C+damage%2C+guard%2C+startup%2C+active%2C+recovery%2C+onBlock%2C+onHit%2C+invuln%2C+type&&order+by=%60chara%60%2C%60input%60%2C%60name%60%2C%60cargo__MoveData_GGST%60.%60images__full%60%2C%60damage%60&limit=1000&format=json";
        static private string imgQuery = "https://dustloop.com/wiki/api.php?action=query&format=json&prop=imageinfo&titles=File:{0}&iiprop=url";
        static private string iconQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=ggstCharacters&&fields=name%2Cicon&&order+by=%60name%60%2C%60icon%60&limit=1000&format=json";
        const int LDistance = 3; //google Levenshtein distance for more info
        static readonly int maxResults = FrameDataEmbedBuilder.emoteList.Count; //will play around with this to see how bad it gets

        static public List<MoveData> dataSource;
        static public List<IconData> iconSource;

        public static void Initialize()
        {
            var response = new WebClient().DownloadString(mainQuery);
            dataSource = JsonConvert.DeserializeObject<List<MoveData>>(response);

            response = new WebClient().DownloadString(iconQuery);
            iconSource = JsonConvert.DeserializeObject<List<IconData>>(response);
            Console.WriteLine("Data loaded");

            //preload images and icons, takes a while but makes requests much faster, disabled while debugging
#if !DEBUG
            foreach (var dataMove in dataSource) //load images
                loadImage(dataMove);
            Console.WriteLine("Images loaded");
#endif
            foreach (var icon in iconSource) //load icons
                loadIcon(icon);
            Console.WriteLine("Icons loaded");
        }

        private static void loadImage(MoveData dataMove)
        {
            if (!dataMove.imgLoaded)
            {
                try
                {
                    var r = new WebClient().DownloadString(String.Format(imgQuery, dataMove.images[0]));
                    if (JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url") != null) //check if image is available
                    {
                        dataMove.imgFull = JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url").Value<string>(); //I hate this
                        dataMove.imgLoaded = true;
                        Console.WriteLine(string.Format("Image: {0} loaded", dataMove.name != null ? dataMove.name : dataMove.input.ToString()));
                    }
                    else
                        Console.WriteLine(string.Format("Error: Image: {0} failed to load", dataMove.name));
                }
                catch(Exception e)
                {
                    Console.WriteLine(string.Format("Error: " + e.StackTrace + "\nImage: {0} failed to load", dataMove.name != null ? dataMove.name : dataMove.input.ToString()));
                }
            }
        }
        private static void loadIcon(IconData icon)
        {
            if (!icon.iconLoaded)
            {
                try
                {
                    var r = new WebClient().DownloadString(String.Format(imgQuery, icon.icon));
                    if (JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url") != null)
                    {
                        icon.iconFull = JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url").Value<string>(); //I hate this
                        icon.iconLoaded = true;
                        Console.WriteLine(string.Format("Image: {0} loaded", icon.name));
                    }
                    else
                        Console.WriteLine(string.Format("Error: Image: {0} failed to load", icon.name));
                }
                catch
                {
                    Console.WriteLine(string.Format("Error: Image: {0} failed to load", icon.name));
                }
            }
        }

        private static (Character?, string, string) moveShorthand(Character? character, string move, string level)
        {
            move = move.ToLower();
            level = level.ToLower();

            //dp inputs
            if (move == "dp" || Levenshtein.Distance(move, "dragon punch") < LDistance)
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    switch (chara)
                    {
                        case Character.Sol:
                            return (character, "Volcanic Viper", "");
                        case Character.Ky:
                            return (character, "Vapor Thrust", "");
                        case Character.Gio:
                            return (character, "Sol Nascente", "");
                        case Character.Leo:
                            return (character, "Eisen Sturm", "");
                        case Character.Chipp:
                            return (character, "Beta Blade", "");
                    }
                }
            }

            //normal grabs
            if (move == "4d" || move == "6d")
                return (character, "ground throw", "");
            if (move == "j.4d" || move == "j.6d")
                return (character, "air throw", "");

            //command grabs
            if (Levenshtein.Distance(move, "command grab") < LDistance || move == "cmd grab" || move == "cmdgb" ||
                Levenshtein.Distance(move, "command throw") < LDistance || move == "cmd throw" || move == "cmdtr")
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    switch (chara)
                    {
                        case Character.Axl:
                            return (character, "Winter Mantis", "");
                        case Character.Chipp:
                            return (character, "Genrouzan", "");
                        case Character.Faust:
                            return (character, "Snip Snip Snip", "");
                        case Character.Leo:
                            return (character, "Gländzendes Dunkel", "");
                        case Character.May:
                            return (character, "Overhead Kiss", "");
                        case Character.Nago:
                            return (character, "Bloodsucking Universe", "");
                        case Character.Pot:
                            return (character, "Potemkin Buster", "");
                        case Character.Sol:
                            return (character, "Wild Throw", "");
                        case Character.Zato:
                            return (character, "Damned Fang", "");
                        case Character.Jacko:
                            return (character, "Forever Elysion Driver", "");
                        case Character.Ino:
                            return (character, "Megalomania", "");
                    }
                }
            }

            //nago level moves
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    List<string> levelMoves = new List<string>(){
                        "j.h", "2h", "6h", "5h"
                    };
                    if (chara == Character.Nago && levelMoves.Any(s => s.Equals(move)) && level.Length == 0)
                        return (character, move, "level");
                }
            }

            //goldlewis level moves
            {
                List<string> levelMoves = new List<string>() {
                        "thunderbird", "skyfish", "burn it down",
                        "214s", "236s", "236236k"
                    };
                if (levelMoves.GetRange(0, 3).Any(f => Levenshtein.Distance(f, move) < LDistance) ||
                    (levelMoves.GetRange(3, 3).Any(f => f == move) &&
                    character.HasValue ? (Character)character == Character.Goldlewis : false &&
                    level.Length == 0))
                    return (character, move, "level");
            }

            //rensen
            if (Levenshtein.Distance(move, "rensen") < LDistance || Levenshtein.Distance(move, "rensengeki") < LDistance)
                return (character, "sickle flash", "");

            //totsugeki
            if (Levenshtein.Distance(move, "totsugeki") < LDistance)
                return (character, "mr. dolphin", "");

            //heavenly potemkin buster
            if (move == "hpb")
                return (character, "heavenly potemkin buster", "");

            //fdb
            if (move == "fdb" || move == "flick")
                return (character, "f.d.b.", "");

            //hfb
            if (move == "hfb")
                return (character, "hammerfall break", "");

            //hmc
            if (move == "hmc")
                return (character, "heavy mob cemetery", "");

            //fed
            if (move == "fed")
                return (character, "forever elysion driver", "");

            //leap
            if (move == "frog")
                return (character, "leap", "");

            //invite hell
            if (move == "drill")
                return (character, "invite hell", "");

            //rtl
            if (move == "rtl")
                return (character, "ride the lightning", "");

            //stroke the big tree
            if (move == "stbt" || move == "cbt") // don't tell mom
                return (character, "stroke the big tree", "");

            //kamuriyuki / Nago spin special
            if (move == "beyblade")
                return (character, "kamuriyuki", "");

            //behemoth typhoon
            if (Levenshtein.Distance(move, "behemoth") < LDistance ||
                Levenshtein.Distance(move, "behemoth typhoon") < LDistance ||
                move == "bt")
                return (character, "behemoth typhoon", "");

            //zato break the law fix
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    if (chara == Character.Zato && move == "214k")
                        return (character, "214[k]", "");
                }
            }

            //sol nascente fix
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    if (chara == Character.Sol && Levenshtein.Distance(move, "nascente") < LDistance)
                        return (Character.Gio, "sol nascente", "");
                }
            }

            //overdrive search
            if (Levenshtein.Distance(move, "overdrive") < LDistance || move == "super")
                return (character, "super", level);

            return (character, move, level);
        }

        public static MoveListInternal fetchMove(Character? character, string move, string level, bool isNumpad)
        {
            List<MoveData> results1 = new List<MoveData>();
            List<MoveData> results2 = new List<MoveData>();
            //store every matching move, by input or by name
            //foreach (var dataMove in dataSource)
            //{
            //    if (dataMove.input.ToLower() == move.ToLower()) //motion input
            //        results1.Add(dataMove);
            //    else if ((/*character == null || */!isNumpad) && (Levenshtein.Distance(dataMove.name.ToLower(), move.ToLower()) < LDistance)) //typos
            //        results1.Add(dataMove);
            //    else if (dataMove.name.ToLower().Contains(move.ToLower())) //broader match
            //        results1.Add(dataMove);
            //    //prevent overloading
            //    if (results1.Count > 50)
            //        break;
            //}

            (character, move, level) = moveShorthand(character, move, level); // transform move and level based on a list of shorthands

            if (move == "super")
                results1.AddRange(dataSource.Where(f => move == f.type)); // overdrive search
            else if (level.Length == 0)
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input.ToLower() == move) || // numpad notation
                                                        (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance) || //typos
                                                        (f.name != null && f.name.ToLower().Contains(move)) || // direct match
                                                        (move == "5s" && (f.input == "c.S" || f.input == "f.S")))); // 5S fix
            else if (level.Length > 0)
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input.ToLower().Contains(move + " " + level)) || // numpad notation
                                                        (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().EndsWith(level)) || // typos
                                                        (!isNumpad && f.name != null && f.name.ToLower().Contains(move) && f.input.ToLower().EndsWith(level)) || // direct match
                                                        (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().Contains(level)))); // all levels

            //remove moves that don't match the character
            if (character.HasValue)
            {
                Character chara = (Character)character;
                foreach (var dataMove in results1)
                    if (dataMove.chara.ToLower().Contains(chara.GetName().ToLower()))
                        results2.Add(dataMove);
            }
            else
                results2.AddRange(results1);

            //try to load an image if it's not present
            if (results2.Any(f => !f.imgLoaded))
                foreach (var dataMove in results2.Where(f => !f.imgLoaded))
                    loadImage(dataMove);

            //construct the result
            MoveListInternal r = new MoveListInternal();
            r.moves = results2;
            if (r.moves.Count == 1)
                r.result = MoveDataResult.Success;
            else if (r.moves.Count == 0)
                r.result = MoveDataResult.NoResult;
            else if (r.moves.Count <= maxResults)
                r.result = MoveDataResult.ExtraResults;
            else if (r.moves.Count == 16 && r.moves[0].name.Contains("Behemoth"))
                r.result = MoveDataResult.SpecialBehemoth;
            else
                r.result = MoveDataResult.TooManyResults;
            return r;
        }

    }
}
