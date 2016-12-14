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
        public static List<ChanThread> chanThreadList = new List<ChanThread>();
        public static string currentBoard;

        public async static Task<List<string>> getBoardList(CancellationToken cancellationToken = new CancellationToken())
        {
            List<string> retVal = new List<string>();

            JArray boardList = 
                (JArray)(
                    (JObject)(
                        await HttpRequest.httpRequestParse(Global.BASE_URL + "boards.json", JObject.Parse, cancellationToken)
                    )
                )["boards"];

            foreach (JObject board in boardList)
            {
                retVal.Add(board["board"].ToString());
            }

            return retVal;
        }

        public async static Task loadBoard(string board)
        {
            chanThreadList.Clear();
            currentBoard = board;

            string address = BASE_URL + board + "/catalog.json";
            JArray boardData = (JArray)await HttpRequest.httpRequestParse(address, JArray.Parse);

            foreach (JObject boardPage in boardData)
            {
                foreach (JObject jsonThread in boardPage["threads"])
                {
                    chanThreadList.Add(new ChanThread(jsonThread));
                }
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
