using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChanBrowserLibrary
{
    class HttpRequest
    {
        public static async Task<string> httpRequest(string url,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var client = new System.Net.Http.HttpClient())
            {
                System.Net.Http.HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return "";
            }
        }

        public static async Task<object> httpRequestParse(string url, Func<string, object> parse,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            string str = await httpRequest(url, cancellationToken);
            return parse(str);
        }
    }
}
