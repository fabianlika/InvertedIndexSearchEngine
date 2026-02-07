namespace InvertedIndexSearchEngine.Server.DTOs
{
    public class SearchResultDto
    {
        public string Title { get; set; }
        public string ContentSnippet { get; set; }
        public double Score { get; set; }
    }
}
