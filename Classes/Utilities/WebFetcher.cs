#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CueTime.Classes.Utilities
{
    internal sealed class WebFetcher
    {
        private const string WeeklyRootUrl = @"https://wol.jw.org/it/wol/meetings/r6/lp-i";
        private const string BaseUrl = "https://wol.jw.org";
        private const string MemorialPageUrl = "https://www.jw.org/it/testimoni-di-geova/commemorazione/";
        private static readonly Uri BaseUri = new Uri(BaseUrl);

        private static readonly HttpClient ClientWinChrome = CreateHttpClient(
            useProxy: false,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

        private static readonly HttpClient ClientMacSafari = CreateHttpClient(
            useProxy: false,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15");

        private static readonly HttpClient ClientProxy = CreateHttpClient(
            useProxy: true,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

        private const int MaxRedirects = 5;

        private static readonly (HttpClient Client, int DelayMs)[] PrimaryFetchStrategies =
        {
            (ClientWinChrome, 0),
            (ClientMacSafari, 350)
        };

        private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "wol.jw.org",
            "jw.org",
            "www.jw.org"
        };

        public async Task<string> GetStringAsync(string url, int timeoutMs)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(timeoutMs);
            Exception? lastException = null;

            foreach ((HttpClient client, int delayMs) in PrimaryFetchStrategies)
            {
                try
                {
                    if (delayMs > 0)
                    {
                        await Task.Delay(delayMs, cts.Token).ConfigureAwait(false);
                    }

                    return await SendAsync(client, url, cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex) when (!cts.IsCancellationRequested)
                {
                    lastException = ex;
                    AppLogger.LogWarning($"Tentativo HTTP fallito verso '{url}' con strategia primaria.", ex);
                }
            }

            try
            {
                return await SendAsync(ClientProxy, url, cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
                AppLogger.LogError($"Fallback proxy fallito verso '{url}'.", ex);
            }

            throw new HttpRequestException($"Impossibile scaricare '{url}' con le strategie configurate.", lastException);
        }

        public async Task<string> LoadWeekPageAsync(int year, int week, int timeoutMs)
        {
            string directUrl = BuildWeekUrl(year, week);

            try
            {
                return await GetStringAsync(directUrl, timeoutMs).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile caricare la pagina settimanale diretta '{directUrl}', uso il fallback root.", ex);
                return await GetStringAsync(WeeklyRootUrl, timeoutMs).ConfigureAwait(false);
            }
        }

        public Task<string> LoadMemorialPageAsync(int timeoutMs)
        {
            return GetStringAsync(MemorialPageUrl, timeoutMs);
        }

        public string BuildAbsoluteUrl(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(href, UriKind.Absolute, out Uri? absolute))
            {
                return absolute.ToString();
            }

            if (Uri.TryCreate(BaseUri, href, out Uri? resolved))
            {
                return resolved.ToString();
            }

            return BaseUrl + href;
        }

        public bool TryBuildAllowedCachedUrl(string href, out string url)
        {
            return TryBuildAllowedUrl(href, out url);
        }

        public bool TryBuildAllowedUrl(string href, out string url)
        {
            url = string.Empty;

            if (string.IsNullOrWhiteSpace(href))
            {
                return false;
            }

            if (!Uri.TryCreate(href, UriKind.Absolute, out Uri? absolute))
            {
                url = BuildAbsoluteUrl(href);
                return IsAllowedCachedUrl(url);
            }

            url = absolute.ToString();
            return IsAllowedCachedUrl(url);
        }

        internal static bool IsAllowedCachedUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttps && AllowedHosts.Contains(uri.Host);
        }

        private static async Task<string> SendAsync(HttpClient client, string url, CancellationToken cancellationToken)
        {
            if (!IsAllowedCachedUrl(url))
            {
                throw new InvalidOperationException($"L'URL '{url}' non è nella allowlist autorizzata.");
            }

            Uri currentUri = new Uri(url, UriKind.Absolute);

            for (int redirectCount = 0; ; redirectCount++)
            {
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, currentUri);
                using HttpResponseMessage resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (IsRedirectStatus(resp.StatusCode))
                {
                    if (redirectCount >= MaxRedirects)
                    {
                        throw new HttpRequestException($"Numero massimo di redirect superato per '{url}'.");
                    }

                    Uri? location = resp.Headers.Location;
                    if (location == null)
                    {
                        throw new HttpRequestException($"Redirect senza Location valido per '{currentUri}'.");
                    }

                    Uri nextUri = location.IsAbsoluteUri ? location : new Uri(currentUri, location);
                    if (!IsAllowedCachedUrl(nextUri.ToString()))
                    {
                        throw new HttpRequestException($"Redirect fuori allowlist bloccato verso '{nextUri}'.");
                    }

                    currentUri = nextUri;
                    continue;
                }

                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static string BuildWeekUrl(int year, int week)
        {
            return $"{WeeklyRootUrl}/{year}/{week:D2}";
        }

        private static HttpClient CreateHttpClient(bool useProxy, string userAgent)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseProxy = useProxy,
                Proxy = null,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                AllowAutoRedirect = false
            };

            HttpClient client = new HttpClient(handler, disposeHandler: true);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");
            return client;
        }

        private static bool IsRedirectStatus(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.MovedPermanently
                || statusCode == HttpStatusCode.Found
                || statusCode == HttpStatusCode.SeeOther
                || statusCode == HttpStatusCode.TemporaryRedirect
                || statusCode == HttpStatusCode.PermanentRedirect;
        }
    }
}

