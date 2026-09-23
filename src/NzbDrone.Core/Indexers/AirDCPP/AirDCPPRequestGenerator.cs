using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPRequestGenerator : IIndexerRequestGenerator
    {
        private const string AccentedChars = "ÀÁÂÃÄÅàáâãäåÈÉÊËèéêëÌÍÎÏìíîïÒÓÔÕÖØòóôõöøÙÚÛÜùúûüÝýÿÑñÇç";
        private const string PlainChars = "AAAAAAaaaaaaEEEEeeeeIIIIiiiiOOOOOOooooooUUUUuuuuYyyNnCc";

        public string BaseUrl { get; set; }
        public AirDCPPSettings Settings { get; set; }

        protected readonly IHttpClient _httpClient;
        protected readonly IAirDCPPProxy _airDCPPProxy;
        protected readonly Logger _logger;

        public AirDCPPRequestGenerator(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _airDCPPProxy = new AirDCPPProxy(_httpClient, logger);
            _logger = logger;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("Anna 1080p Bluray"));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.Series.Title, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        private IndexerPageableRequestChain GetDefaultSearchRequest(string series, List<(int Season, int Episode)> episodes)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var episode in episodes)
            {
                var searchString = string.Format("{0} S{1:00}E{2:00}", RemoveAccents(series), episode.Season, episode.Episode);
                pageableRequests.Add(GetRequest(searchString));
            }

            return pageableRequests;
        }

        private static string RemoveAccents(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var builder = new StringBuilder(input.Length);

            foreach (var character in input)
            {
                var index = AccentedChars.IndexOf(character);
                builder.Append(index >= 0 ? PlainChars[index] : character);
            }

            return builder.ToString();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.Series.Title, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.Series.Title, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.Series.Title, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(AnimeSeasonSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public class CustomInfo
        {
            public QueryInfo query { get; set; }
        }

        public class QueryInfo
        {
            public string pattern { get; set; }
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchName)
        {
            var request = _airDCPPProxy.PerformSearch(Settings, searchName);
            yield return new IndexerRequest(request);
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
