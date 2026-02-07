using InvertedIndexSearchEngine.Server.DTOs;
using InvertedIndexSearchEngine.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace InvertedIndexSearchEngine.Services
{
    public class SearchService
    {
        private readonly SearchDbContext _context;

        public SearchService(SearchDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs a TF-IDF based search on the indexed documents.
        /// Returns a list of SearchResultDto with title, snippet, and score.
        /// </summary>
        public async Task<List<SearchResultDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<SearchResultDto>();

            var queryTerms = query.ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            int totalDocs = await _context.Documents.CountAsync();

            var termIds = await _context.Terms
                .Where(t => queryTerms.Contains(t.Word))
                .Select(t => new { t.Id, t.Word })
                .ToListAsync();

            if (!termIds.Any())
                return new List<SearchResultDto>();

            var indexEntries = await _context.InvertedIndices
                .Where(ii => termIds.Select(t => t.Id).Contains(ii.TermId))
                .Include(ii => ii.Document)
                .ToListAsync();

            var groupedDocs = indexEntries.GroupBy(ii => ii.Document);

            var results = new List<SearchResultDto>();

            foreach (var group in groupedDocs)
            {
                // AND logic: only documents containing all query terms
                if (group.Select(g => g.TermId).Distinct().Count() < queryTerms.Count)
                    continue;

                double score = 0;

                foreach (var entry in group)
                {
                    int docFreq = await _context.InvertedIndices
                        .CountAsync(x => x.TermId == entry.TermId);

                    double tf = entry.Frequency ?? 0;
                    double idf = Math.Log10((double)totalDocs / (docFreq + 1));

                    score += tf * idf;
                }

                var doc = group.Key;
                string content = doc.Content ?? "";

                string snippet = content.Length > 150
                    ? content.Substring(0, 150) + "..."
                    : content;

                results.Add(new SearchResultDto
                {
                    Title = doc.Title,
                    ContentSnippet = snippet,
                    Score = Math.Round(score, 4)
                });
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
    }
}
