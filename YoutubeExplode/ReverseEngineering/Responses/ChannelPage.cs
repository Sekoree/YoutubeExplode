using System;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class ChannelPage
    {
        private readonly IHtmlDocument _root;

        public ChannelPage(IHtmlDocument root) => _root = root;

        private bool IsOk() => _root
            .QuerySelector("meta[property=\"og:url\"]") is not null;

        public string GetChannelUrl() => _root
            .QuerySelectorOrThrow("meta[property=\"og:url\"]")
            .GetAttributeOrThrow("content");

        public string GetChannelId() => GetChannelUrl()
            .SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase);

        public string GetChannelTitle() => _root
            .QuerySelectorOrThrow("meta[property=\"og:title\"]")
            .GetAttributeOrThrow("content");

        public string GetChannelLogoUrl() => _root
            .QuerySelectorOrThrow("meta[property=\"og:image\"]")
            .GetAttributeOrThrow("content");
    }

    internal partial class ChannelPage
    {
        public static ChannelPage Parse(string raw) => new(Html.Parse(raw));

        public static async Task<ChannelPage> GetAsync(YoutubeHttpClient httpClient, string id) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://www.youtube.com/channel/{id}?hl=en";
                var raw = await httpClient.GetStringAsync(url);

                var result = Parse(raw);

                if (!result.IsOk())
                    throw TransientFailureException.Generic("Channel page is broken.");

                return result;
            });

        public static async Task<ChannelPage> GetByUserNameAsync(YoutubeHttpClient httpClient, string userName) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://www.youtube.com/user/{userName}?hl=en";
                var raw = await httpClient.GetStringAsync(url);

                var result = Parse(raw);

                if (!result.IsOk())
                    throw TransientFailureException.Generic("Channel page is broken.");

                return result;
            });
    }
}