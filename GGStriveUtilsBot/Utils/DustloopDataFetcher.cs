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
        static private string mainQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=MoveData_GGST&&fields=chara%2C+input%2C+name%2C+images%2C+damage%2C+guard%2C+startup%2C+active%2C+recovery%2C+onBlock%2C+onHit%2C+invuln&&order+by=%60chara%60%2C%60input%60%2C%60name%60%2C%60cargo__MoveData_GGST%60.%60images__full%60%2C%60damage%60&limit=1000&format=json";
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
            {
                loadImage(dataMove);
                Console.WriteLine(string.Format("Image: {0} loaded", dataMove.imgFull));
            }
            Console.WriteLine("Images loaded");
#endif
            foreach (var icon in iconSource) //load icons
            {
                loadIcon(icon);
                Console.WriteLine(string.Format("Icon for {0} loaded", icon.name));
            }
            Console.WriteLine("Icons loaded");
        }

        private static void loadImage(MoveData dataMove)
        {
            if (!dataMove.imgLoaded)
            {
                var r = new WebClient().DownloadString(String.Format(imgQuery, dataMove.images[0]));
                if (JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url") != null) //check if image is available
                {
                    dataMove.imgFull = JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url").Value<string>(); //I hate this
                    dataMove.imgLoaded = true;
                }
            }
        }
        private static void loadIcon(IconData icon)
        {
            if (!icon.iconLoaded)
            {
                var r = new WebClient().DownloadString(String.Format(imgQuery, icon.icon));
                if (JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url") != null)
                {
                    icon.iconFull = JObject.Parse(r).SelectToken("query.pages.*.imageinfo[0].url").Value<string>(); //I hate this
                    icon.iconLoaded = true;
                }
            }
        }

        private static (string, string) moveShorthand(Character? character, string move, string level)
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
                            return ("Volcanic Viper", "");
                        case Character.Ky:
                            return ("Vapor Thrust", "");
                        case Character.Gio:
                            return ("Sol Nascente", "");
                        case Character.Leo:
                            return ("Eisen Sturm", "");
                        case Character.Chipp:
                            return ("Beta Blade", "");
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
                        return (move, "level");
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
                    return (move, "level");
            }
            //totsugeki
            if (Levenshtein.Distance(move, "totsugeki") < LDistance)
                return ("mr. dolphin", "");
            //heavenly potemkin buster
            if (move == "hpb")
                return ("heavenly potemkin buster", "");
            //fdb
            if (move == "fdb")
                return ("f.d.b.", "");
            //behemoth typhoon
            if (Levenshtein.Distance(move, "behemoth") < LDistance || Levenshtein.Distance(move, "behemoth typhoon") < LDistance)
                return ("behemoth typhoon", "");
            //zato break the law fix
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    if (chara == Character.Zato && move == "214k")
                        return ("214[k]", "");
                }
            }

            return (move, level);
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

            (move, level) = moveShorthand(character, move, level); // transform move and level based on a list of shorthands

            if (level.Length == 0)
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input.ToLower() == move) || // numpad notation
                                                        (!isNumpad && Levenshtein.Distance(f.name.ToLower(), move) < LDistance) || //typos
                                                        (f.name.ToLower().Contains(move)) || // direct match
                                                        (move == "5s" && (f.input == "c.S" || f.input == "f.S")))); // 5S fix
            else
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input.ToLower().Contains(move + " " + level)) || // numpad notation
                                                        (!isNumpad && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().EndsWith(level)) || // typos
                                                        (!isNumpad && f.name.ToLower().Contains(move) && f.input.ToLower().EndsWith(level)) || // direct match
                                                        (!isNumpad && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().Contains(level)))); // all levels

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
            //load the image, if debugging
#if DEBUG
            foreach (var dataMove in results2)
                loadImage(dataMove);
#endif
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
