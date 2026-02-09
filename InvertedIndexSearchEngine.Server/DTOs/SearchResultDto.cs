namespace InvertedIndexSearchEngine.Server.DTOs
{
    public class SearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ContentSnippet { get; set; }
        public double Score { get; set; }
    }
}
