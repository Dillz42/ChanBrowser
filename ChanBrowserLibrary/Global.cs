using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChanBrowserLibrary
{
    public class Global
    {
        public const string BASE_URL = "http://a.4cdn.org/";
        public const string BASE_IMAGE_URL = "http://i.4cdn.org/";
        public const string DEFAULT_IMAGE = "http://s.4cdn.org/image/fp/logo-transparent.png";
        public static List<ChanPost> chanThreadList = new List<ChanPost>();
        public static string currentBoard;

        public async static Task<List<Tuple<string, string>>> getBoardList(CancellationToken cancellationToken = new CancellationToken())
        {
            List<Tuple<string, string>> retVal = new List<Tuple<string, string>>();

            JArray boardList = 
                (JArray)(
                    (JObject)(
                        await HttpRequest.httpRequestParse(Global.BASE_URL + "boards.json", JObject.Parse, cancellationToken)
                    )
                )["boards"];

            foreach (JObject board in boardList)
            {
                retVal.Add(Tuple.Create(
                    board["board"].ToString(), 
                    board["title"].ToString() + "\n" + System.Net.WebUtility.HtmlDecode(board["meta_description"].ToString())));
            }

            return retVal;
        }

        public async static Task loadBoard(string board, CancellationToken cancellationToken = new CancellationToken())
        {
            chanThreadList.Clear();
            currentBoard = board;

            string address = BASE_URL + board + "/catalog.json";
            JArray boardData = (JArray)await HttpRequest.httpRequestParse(address, JArray.Parse);

            foreach (JObject boardPage in boardData)
            {
                foreach (JObject jsonThread in boardPage["threads"])
                {
                    chanThreadList.Add(new ChanPost(jsonThread, board));
                }
            }

            chanThreadList.Sort((ChanPost a, ChanPost b) => { return b.last_modified - a.last_modified; });
        }

        public async static Task loadThread(ChanPost op, CancellationToken cancellationToken = new CancellationToken())
        {
            string address = BASE_URL + op.board + "/thread/" + op.no + ".json";

            JObject thread = (JObject)await HttpRequest.httpRequestParse(address, JObject.Parse);

            foreach (JObject post in thread["posts"])
            {
                op.replyList.Add(new ChanPost(post, op.board));
            }

        }

        public static void htmlToTextBlockText(System.Windows.Controls.TextBlock textBlock, string html)
        {
            try
            {
                textBlock.Text = "";
                string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
                System.Windows.Documents.InlineCollection xamlLines =
                    (
                        (System.Windows.Documents.Paragraph)(
                            (System.Windows.Documents.Section)System.Windows.Markup.XamlReader.Parse(xaml)
                        ).Blocks.FirstBlock
                    ).Inlines;

                System.Windows.Documents.Inline[] newLines = new System.Windows.Documents.Inline[xamlLines.Count];
                xamlLines.CopyTo(newLines, 0);

                foreach (var item in newLines)
                {
                    textBlock.Inlines.Add(item);
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
