﻿using System;
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
        static private string mainQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=MoveData_GGST&&fields=chara%2C+input%2C+name%2C+images%2C+damage%2C+guard%2C+startup%2C+active%2C+recovery%2C+onBlock%2C+onHit%2C+invuln%2C+type&&order+by=%60chara%60%2C%60input%60%2C%60name%60%2C%60cargo__MoveData_GGST%60.%60images__full%60%2C%60damage%60&limit=2000&format=json";
        static private string imgQuery = "https://dustloop.com/wiki/api.php?action=query&format=json&prop=imageinfo&titles=File:{0}&iiprop=url";
        static private string iconQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=ggstCharacters&&fields=name%2Cicon&&order+by=%60name%60%2C%60icon%60&limit=1000&format=json";
        const int LDistance = 3; //google Levenshtein distance for more info
        static readonly int maxResults = FrameDataEmbedBuilder.emoteList.Count; //will play around with this to see how bad it gets

        static public List<MoveData> dataSource;
        static public List<IconData> iconSource;

        public static Task Initialize()
        {
            var response = new WebClient().DownloadString(mainQuery);
            response = response.Remove(0, response.LastIndexOf(">") + 1);
            //Console.Write(response.Substring(0,100));
            dataSource = JsonConvert.DeserializeObject<List<MoveData>>(response);

            response = new WebClient().DownloadString(iconQuery);
            iconSource = JsonConvert.DeserializeObject<List<IconData>>(response);
            Console.WriteLine("Data loaded");

            //replace line breaks with proper line breaks
            var list = dataSource.Where(f => f.invuln != null && f.invuln.Contains("&lt;br/&gt;")).ToList();
            foreach (var move in list)
            {
                move.invuln = move.invuln.Replace("&lt;br/&gt;", "\n");
            }
            Console.WriteLine("Line breaks cleared, found: " + list.Count);

            //preload images and icons, takes a while but makes requests much faster, disabled while debugging
#if !DEBUG
            foreach (var dataMove in dataSource) //load images
                loadImage(dataMove);
            Console.WriteLine("Images loaded");
#endif
            foreach (var icon in iconSource) //load icons
                loadIcon(icon);
            Console.WriteLine("Icons loaded");

            return Task.CompletedTask;
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
                catch (Exception e)
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

        private static (Character?, string, string, bool) moveShorthand(Character? character, string move, string level, bool isNumpad)
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
                            return (character, "volcanic viper", "", false);
                        case Character.Ky:
                            return (character, "vapor thrust", "", false);
                        case Character.Gio:
                            return (character, "sol nascente", "", false);
                        case Character.Leo:
                            return (character, "eisen sturm", "", false);
                        case Character.Chipp:
                            return (character, "beta blade", "", false);
                        case Character.Bridget:
                            return (character, "starship", "", false);
                        case Character.Sin:
                            return (character, "hawk baker", "", false);
                    }
                }
            }

            //normal grabs
            if (move == "4d" || move == "6d")
                return (character, "ground throw", "", false);
            if (move == "j.4d" || move == "j.6d")
                return (character, "air throw", "", false);

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
                            return (character, "winter mantis", "", false);
                        case Character.Chipp:
                            return (character, "genrouzan", "", false);
                        case Character.Faust:
                            return (character, "snip snip snip", "", false);
                        case Character.Leo:
                            return (character, "gländzendes dunkel", "", false);
                        case Character.May:
                            return (character, "overhead kiss", "", false);
                        case Character.Nago:
                            return (character, "bloodsucking universe", "", false);
                        case Character.Pot:
                            return (character, "potemkin buster", "", false);
                        case Character.Sol:
                            return (character, "wild throw", "", false);
                        case Character.Zato:
                            return (character, "damned fang", "", false);
                        case Character.Bridget:
                            return (character, "rock the baby", "", false);
                        case Character.Jacko:
                            return (character, "forever elysion driver", "", false);
                        case Character.Ino:
                            return (character, "megalomania", "", false);
                    }
                }
            }

            //nago level moves
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    List<string> levelMoves = new List<string>(){
                        "f.s", "f.ss", "f.sss", "5h", "2s", "2h", "j.s", "j.h", "j.d", "6h"
                    };
                    if (chara == Character.Nago && levelMoves.Any(s => s.Equals(move)) && level.Length == 0)
                        return (character, move, "level", isNumpad);
                }
            }

            //goldlewis level moves
            {
                List<string> levelMoves = new List<string>() {
                        "thunderbird", "skyfish", "burn it down",
                        "214s", "236s", "236236k"
                    };
                if ((levelMoves.GetRange(0, 3).Any(f => Levenshtein.Distance(f, move) < LDistance) ||
                    (levelMoves.GetRange(3, 3).Any(f => f == move) && character.HasValue ? (Character)character == Character.Goldlewis : false)) &&
                    level.Length == 0)
                    return (character, move, "level", isNumpad);
            }

            //bt shorthands
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    if (chara == Character.Goldlewis)
                    {
                        //ground
                        if (move == "842h")
                            return (character, "87412h", "", true);
                        if (move == "862h")
                            return (character, "89632h", "", true);
                        if (move == "486h")
                            return (character, "47896h", "", true);
                        if (move == "684h")
                            return (character, "69874h", "", true);
                        if (move == "426h")
                            return (character, "41236h", "", true);
                        if (move == "624h")
                            return (character, "63214h", "", true);
                        if (move == "248h")
                            return (character, "21478h", "", true);
                        if (move == "268h")
                            return (character, "23698h", "", true);
                        //air
                        var m = move.Replace(".", string.Empty);
                        if (m == "j842h")
                            return (character, "j.87412h", "", true);
                        if (m == "j862h")
                            return (character, "j.89632h", "", true);
                        if (m == "j486h")
                            return (character, "j.47896h", "", true);
                        if (m == "j684h")
                            return (character, "j.69874h", "", true);
                        if (m == "j426h")
                            return (character, "j.41236h", "", true);
                        if (m == "j624h")
                            return (character, "j.63214h", "", true);
                        if (m == "j248h")
                            return (character, "j.21478h", "", true);
                        if (m == "j268h")
                            return (character, "j.23698h", "", true);
                    }
                }
            }

            //rensen
            if (Levenshtein.Distance(move, "rensen") < LDistance || Levenshtein.Distance(move, "rensengeki") < LDistance)
                return (character, "sickle flash", "", false);

            //totsugeki
            if (Levenshtein.Distance(move, "totsugeki") < LDistance)
                return (character, "mr. dolphin", "", false);

            //heavenly potemkin buster
            if (move == "hpb")
                return (character, "heavenly potemkin buster", "", false);

            //fdb
            if (move == "fdb" || move == "flick")
                return (character, "f.d.b.", "", false);

            //hfb
            if (move == "hfb")
                return (character, "hammerfall break", "", false);

            //hmc
            if (move == "hmc")
                return (character, "heavy mob cemetery", "", false);

            //fed
            if (move == "fed")
                return (character, "forever elysion driver", "", false);

            //leap
            if (move == "frog")
                return (character, "leap", "", false);

            //invite hell
            if (move == "drill")
                return (character, "invite hell", "", false);

            //that's a lot
            if (move == "drills")
                return (character, "that's a lot", "", false);

            //rtl
            if (move == "rtl")
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;
                    if (chara == Character.Sin)
                    {
                        return (character, "r.t.l", "", false);
                    }
                }
                return (character, "ride the lightning", "", false);
            }
            //calvados
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Ram)
                    if (move == "beam")
                        return (character, "calvados", "", true);
            }

            //mortobato
            if (move == "motorboat")
                return (character, "mortobato", "", false);

            //hop
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Ram)
                    if (move == "hop")
                        return (character, "214k", "", true);
                if (chara == Character.Anji)
                    if (move == "hop")
                        return (character, "236h k", "", true);
            }

            //servant shoot
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Jacko)
                    if (move == "kick")
                        return (character, "236k", "", true);
            }

                //volcanic viper
                if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Sol)
                {
                    if (move == "svv")
                        return (Character.Sol, "623s", "", true);
                    if (move == "hvv")
                        return (Character.Sol, "623h", "", true);
                    {
                        var m = move.Replace(".", string.Empty);
                        if (m == "jsvv")
                            return (Character.Sol, "j.623s", "", true);
                        if (m == "jhvv")
                            return (Character.Sol, "j.623h", "", true);
                    }
                }
            }
            else
            {
                if (move == "svv")
                    return (Character.Sol, "623s", "", true);
                if (move == "hvv")
                    return (Character.Sol, "623h", "", true);
                {
                    var m = move.Replace(".", string.Empty);
                    if (m == "jsvv")
                        return (Character.Sol, "j.623s", "", true);
                    if (m == "jhvv")
                        return (Character.Sol, "j.623h", "", true);
                }
            }

            //stroke the big tree
            if (move == "stbt" || move == "cbt") // don't tell mom
                return (character, "stroke the big tree", "", false);

            //more stbt shorthands
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Ino)
                {
                    if (move == "s stroke" || move == "slash stroke")
                        return (character, "236s", "", true);
                    if (move == "h stroke" || move == "hs stroke" || move == "heavy stroke" || move == "heavy slash stroke" || move == "-2")
                        return (character, "236h", "", true);
                }
            }
            else
            {
                if (move == "s stroke" || move == "slash stroke")
                    return (Character.Ino, "236s", "", true);
                if (move == "h stroke" || move == "hs stroke" || move == "heavy stroke" || move == "heavy slash stroke" || move == "-2")
                    return (Character.Ino, "236h", "", true);
            }

            //i-no note
            if (move == "note")
                return (Character.Ino, "antidepressant scale", "", false);

            //dolphins
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.May)
                {
                    if (move == "s dolphin" || move == "slash dolphin")
                        return (character, "[4]6s", "", true);
                    if (move == "h dolphin" || move == "hs dolphin" || move == "heavy dolphin" || move == "heavy slash dolphin")
                        return (character, "[4]6h", "", true);
                }
            }
            else
            {
                if (move == "s dolphin" || move == "slash dolphin")
                    return (Character.May, "[4]6s", "", true);
                if (move == "h dolphin" || move == "hs dolphin" || move == "heavy dolphin" || move == "heavy slash dolphin")
                    return (Character.May, "[4]6h", "", true);
            }

            //arbiter
            if (character.HasValue)
            {
                Character chara = (Character)character;
                if (chara == Character.Testament)
                {
                    if (move == "s arbiter" || move == "slash arbiter" || move == "s arbiter sign" || move == "slash arbiter sign")
                        return (character, "236s", "", true);
                    if (move == "h arbiter" || move == "hs arbiter" || move == "heavy arbiter" || move == "heavy slash arbiter" ||
                        move == "h arbiter sign" || move == "hs arbiter sign" || move == "heavy arbiter sign" || move == "heavy slash arbiter sign")
                        return (character, "236h", "", true);
                }
            }
            else
            {
                if (move == "s arbiter" || move == "slash arbiter" || move == "s arbiter sign" || move == "slash arbiter sign")
                    return (Character.Testament, "236s", "", true);
                if (move == "h arbiter" || move == "hs arbiter" || move == "heavy arbiter" || move == "heavy slash arbiter" ||
                    move == "h arbiter sign" || move == "hs arbiter sign" || move == "heavy arbiter sign" || move == "heavy slash arbiter sign")
                    return (Character.Testament, "236h", "", true);
            }

            //kamuriyuki / Nago spin special
            if (move == "beyblade")
                return (character, "kamuriyuki", "", false);

            //behemoth typhoon
            if (Levenshtein.Distance(move, "behemoth") < LDistance ||
                Levenshtein.Distance(move, "behemoth typhoon") < LDistance ||
                move == "bt")
                return (character, "behemoth typhoon", "", false);

            //resshou (chipp rekka mid)
            if (Levenshtein.Distance(move, "sushi") < LDistance)
                return (character, "resshou", "", false);

            //rokusai (chipp rekka low)
            if (Levenshtein.Distance(move, "sukiyaki") < LDistance)
                return (character, "rokusai", "", false);

            //senshuu (chipp rekka overhead)
            if (Levenshtein.Distance(move, "banzai") < LDistance)
                return (character, "senshuu", "", false);

            //character-specific fixes
            {
                if (character.HasValue)
                {
                    Character chara = (Character)character;

                    //zato break the law fix
                    if (chara == Character.Zato && move == "214k")
                        return (character, "214[k]", "", isNumpad);
                    //gio sol nascente fix
                    else if (chara == Character.Sol && Levenshtein.Distance(move, "nascente") < LDistance)
                        return (Character.Gio, "sol nascente", "", isNumpad);
                    //leo parry attack fix
                    else if (chara == Character.Leo && (
                             move == "[s]h" || move == "[h]s" ||
                             move == "[s/h]h/s" || move == "guard attack"))
                        return (character, "[s/h] h/s", "", true);
                    //zarameyuki (nago clone)
                    else if (chara == Character.Nago && move == "clone")
                        return (character, "zarameyuki", "", isNumpad);
                    //gamma blade (chipp clone)
                    else if (chara == Character.Chipp && move == "clone")
                        return (character, "gamma blade", "", isNumpad);
                    //scapegoat (happy-chaos clone)
                    else if (chara == Character.HappyChaos && move == "clone")
                        return (character, "scapegoat", "", isNumpad);
                }
            }

            //overdrive search
            if (Levenshtein.Distance(move, "overdrive") < LDistance || move == "super")
                return (character, "super", level, isNumpad);

            return (character, move, level, isNumpad);
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

            (character, move, level, isNumpad) = moveShorthand(character, move, level, isNumpad); // transform move and level based on a list of shorthands

            if (move == "super")
                results1.AddRange(dataSource.Where(f => move == f.type)); // overdrive search
            else if (level.Length == 0)
            {
                // Check Exact matches
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input != null && f.input.ToLower() == move) || // numpad & literal exact matches
                                                        (!isNumpad && f.name != null && f.name.ToLower().Contains(move)) ||
                                                        (move == "5s" && (f.input == "c.S" || f.input == "f.S"))));   // Also 5S fix

                // Check Levenshtein if no exact match
                if (results1.Count == 0)
                {
                    results1.AddRange(dataSource.Where(f => (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance)));
                }
            }
            else if (level.Length > 0)
            {
                // Check exact matches
                results1.AddRange(dataSource.Where(f => (isNumpad && f.input != null && f.input.ToLower().Contains(move + " " + level)) || // numpad & literal exact matches
                                                        (!isNumpad && f.name != null && f.name.ToLower().Contains(move) && f.input.ToLower().EndsWith(level))));
                // Check Levenshtein if no exact match
                if (results1.Count == 0)
                {
                    results1.AddRange(dataSource.Where(f => (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().EndsWith(level)) || // typos
                                                        (!isNumpad && f.name != null && Levenshtein.Distance(f.name.ToLower(), move) < LDistance && f.input.ToLower().Contains(level)))); // all levels
                }
            }
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
