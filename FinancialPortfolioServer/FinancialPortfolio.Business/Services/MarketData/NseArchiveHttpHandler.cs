using System.Net;

namespace FinancialPortfolio.Business.Services.MarketData
{
    /// <summary>
    /// NSE archives require a browser-like cookie from nseindia.com before file downloads.
    /// </summary>
    public sealed class NseArchiveHttpHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private DateTimeOffset _warmedAt = DateTimeOffset.MinValue;

        public NseArchiveHttpHandler()
        {
            InnerHandler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AllowAutoRedirect = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await EnsureWarmedAsync(cancellationToken);
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task EnsureWarmedAsync(CancellationToken cancellationToken)
        {
            if (DateTimeOffset.UtcNow - _warmedAt < TimeSpan.FromMinutes(8))
                return;

            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (DateTimeOffset.UtcNow - _warmedAt < TimeSpan.FromMinutes(8))
                    return;

                foreach (var url in new[]
                         {
                             "https://www.nseindia.com/",
                             "https://www.nseindia.com/all-reports"
                         })
                {
                    using var warmup = new HttpRequestMessage(HttpMethod.Get, url);
                    warmup.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml");
                    warmup.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                    try
                    {
                        using var response = await base.SendAsync(warmup, cancellationToken);
                        _ = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    }
                    catch
                    {
                        // Homepage warmup is best-effort; archive calls may still succeed.
                    }
                }

                _warmedAt = DateTimeOffset.UtcNow;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
