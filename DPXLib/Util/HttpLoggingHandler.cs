using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DPXLib.Util
{
    /// <summary>
    /// http handler that logs requests and responses to <see cref="Debug"/> and the LogWriter
    /// Taken from  https://github.com/reactiveui/refit/issues/258
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        public TextWriter LogWriter { get; set; }

        public HttpLoggingHandler(HttpMessageHandler innerHandler = null)
            : base(innerHandler ?? new HttpClientHandler())
        { }

        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var req = request;
            var id = Guid.NewGuid().ToString();
            var msg = $"[{id}   Request]";

            WriteLine($"{msg}========Start==========");
            WriteLine($"{msg} {req.Method} {req.RequestUri.PathAndQuery} {req.RequestUri.Scheme}/{req.Version}");
            WriteLine($"{msg} Host: {req.RequestUri.Scheme}://{req.RequestUri.Host}");

            foreach (var header in req.Headers)
                WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

            if (req.Content != null)
            {
                foreach (var header in req.Content.Headers)
                    WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

                if (req.Content is StringContent || this.IsTextBasedContentType(req.Headers) || this.IsTextBasedContentType(req.Content.Headers))
                {
                    var result = await req.Content.ReadAsStringAsync();

                    WriteLine($"{msg} Content:");
                    WriteLine($"{msg} {string.Join("", result.Cast<char>().Take(255))}...");

                }
            }

            var start = DateTime.Now;

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var end = DateTime.Now;

            WriteLine($"{msg} Duration: {end - start}");
            WriteLine($"{msg}==========End==========");

            msg = $"[{id}   Response]";
            WriteLine($"{msg}=========Start=========");

            var resp = response;

            WriteLine($"{msg} {req.RequestUri.Scheme.ToUpper()}/{resp.Version} {(int)resp.StatusCode} {resp.ReasonPhrase}");

            foreach (var header in resp.Headers)
                WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

            if (resp.Content != null)
            {
                foreach (var header in resp.Content.Headers)
                    WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

                if (resp.Content is StringContent || this.IsTextBasedContentType(resp.Headers) || this.IsTextBasedContentType(resp.Content.Headers))
                {
                    start = DateTime.Now;
                    var result = await resp.Content.ReadAsStringAsync();
                    end = DateTime.Now;

                    WriteLine($"{msg} Content:");
                    WriteLine($"{msg} {string.Join("", result.Cast<char>().Take(255))}...");
                    WriteLine($"{msg} Duration: {end - start}");
                }
            }

            WriteLine($"{msg}==========End==========");
            return response;
        }

        readonly string[] types = new[] { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };

        bool IsTextBasedContentType(HttpHeaders headers)
        {
            if (!headers.TryGetValues("Content-Type", out IEnumerable<string> values))
                return false;
            var header = string.Join(" ", values).ToLowerInvariant();

            return types.Any(t => header.Contains(t));
        }

        protected override void Dispose(bool disposing)
        {
            LogWriter?.Flush();
            LogWriter?.Dispose();
            base.Dispose(disposing);
        }

        void WriteLine(string ln)
        {
            Debug.WriteLine(ln);
            LogWriter?.WriteLine(ln);
            LogWriter?.Flush();
        }
    }
}