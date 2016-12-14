using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChanBrowserLibrary
{
    public class Global
    {
        public const string BASE_URL = "http://a.4cdn.org/";
        public const string BASE_IMAGE_URL = "http://i.4cdn.org/";
        public static List<ChanThread> chanThreadList = new List<ChanThread>();

        public async static Task<List<string>> getBoardList()
        {
            List<string> retVal = new List<string>();

            JArray boardList = (JArray)((JObject)(await HttpRequest.httpRequestParse(Global.BASE_URL + "boards.json", JObject.Parse)))["boards"];
            foreach (JObject board in boardList)
            {
                retVal.Add(board["board"].ToString());
            }

            return retVal;
        }
    }
}
