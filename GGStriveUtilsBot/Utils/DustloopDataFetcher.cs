using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using Fastenshtein;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGStriveUtilsBot.Utils
{
    static class DustloopDataFetcher
    {
        static private string mainQuery = "https://www.dustloop.com/wiki/index.php?title=Special:CargoExport&tables=MoveData_GGST&&fields=chara%2C+input%2C+name%2C+images%2C+damage%2C+guard%2C+startup%2C+active%2C+recovery%2C+onBlock%2C+onHit%2C+invuln&&order+by=%60chara%60%2C%60input%60%2C%60name%60%2C%60cargo__MoveData_GGST%60.%60images__full%60%2C%60damage%60&limit=1000&format=json";
        static private string imgQuery = "https://dustloop.com/wiki/api.php?action=query&format=json&prop=imageinfo&titles=File:{0}&iiprop=url";
        const int LDistance = 3; //google Levenshtein distance for more info
        const int maxResults = 4; //will play around with this to see how bad it gets

        static public List<MoveData> dataSource;

        public static void Initialize()
        {
            var response = new WebClient().DownloadString(mainQuery);
            dataSource = JsonConvert.DeserializeObject<List<MoveData>>(response);
            Console.WriteLine("Data loaded");
        }

        public static MoveListInternal fetchMove(string character, string move)
        {
            List<MoveData> results1 = new List<MoveData>();
            List<MoveData> results2 = new List<MoveData>();
            //store every matching move, by input or by name
            foreach (var dataMove in dataSource)
            {
                if (dataMove.input.ToLower() == move.ToLower()) //motion input
                    results1.Add(dataMove);
                else if (Levenshtein.Distance(dataMove.name.ToLower(), move.ToLower()) < LDistance) //typos
                    results1.Add(dataMove);
                else if(dataMove.name.ToLower().Contains(move.ToLower())) //broader match
                    results1.Add(dataMove);
                //prevent overloading
                if (results1.Count > 10)
                    break;
            }
            //remove moves that don't match the character
            if (character != null)
            {
                foreach (var dataMove in results1)
                {
                    if (dataMove.chara.ToLower().Contains(character.ToLower()))
                        results2.Add(dataMove);
                }
            }
            //load the image
            foreach(var dataMove in results2)
            {
                if (!dataMove.imgLoaded)
                {
                    var response = new WebClient().DownloadString(String.Format(imgQuery, dataMove.images[0]));
                    dataMove.imgFull = JObject.Parse(response).SelectToken("query.pages.*.imageinfo[0].url").Value<string>(); //I hate this
                    dataMove.imgLoaded = true;
                }
            }
            //construct the result
            MoveListInternal r = new MoveListInternal();
            r.moves = results2;
            if (r.moves.Count == 1)
                r.result = MoveDataResult.Success;
            else if (r.moves.Count == 0)
                r.result = MoveDataResult.NoResult;
            else if (r.moves.Count <= maxResults)
                r.result = MoveDataResult.ExtraResults;
            else
                r.result = MoveDataResult.TooManyResults;
            return r;
        }

    }
}
