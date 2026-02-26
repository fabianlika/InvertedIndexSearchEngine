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
            // If query is empty or whitespace, return no results immediately
            if (string.IsNullOrWhiteSpace(query))
                return new List<SearchResultDto>();

            // Normalize query:
            // - lowercase
            // - split into words
            // - remove empty entries
            // - remove duplicate terms
            var queryTerms = query.ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            // Total number of documents (used later for IDF calculation)
            int totalDocs = await _context.Documents.CountAsync();

            // Find term IDs in DB that match query words
            // Example: "database search" -> Terms table -> [databaseId, searchId]
            var termIds = await _context.Terms
                .Where(t => queryTerms.Contains(t.Word))
                .Select(t => new { t.Id, t.Word })
                .ToListAsync();

            // If none of the query words exist in index → return no results
            if (!termIds.Any())
                return new List<SearchResultDto>();

            // Fetch inverted index entries where TermId matches query terms
            // Includes the related Document entity
            var indexEntries = await _context.InvertedIndices
                .Where(ii => termIds.Select(t => t.Id).Contains(ii.TermId))
                .Include(ii => ii.Document)
                .ToListAsync();

            // Group index entries by Document
            // This lets us process results per document
            var groupedDocs = indexEntries.GroupBy(ii => ii.Document);

            var results = new List<SearchResultDto>();

            // Loop through each document group
            foreach (var group in groupedDocs)
            {
                // AND logic:
                // Skip documents that do NOT contain ALL query terms
                // Example: query = "database index"
                // If document contains only "database" → skip it
                if (group.Select(g => g.TermId).Distinct().Count() < queryTerms.Count)
                    continue;

                double score = 0;

                // Compute TF-IDF score per matching term in document
                foreach (var entry in group)
                {
                    // Document Frequency:
                    // How many documents contain this term?
                    int docFreq = await _context.InvertedIndices
                        .CountAsync(x => x.TermId == entry.TermId);

                    // Term Frequency (TF):
                    // How often this term appears in this document
                    double tf = entry.Frequency ?? 0;

                    // Inverse Document Frequency (IDF):
                    // Penalizes common words, boosts rare words
                    double idf = Math.Log10((double)totalDocs / (docFreq + 1));

                    // Accumulate score = TF × IDF
                    score += tf * idf;
                }

                // Retrieve document entity
                var doc = group.Key;
                string content = doc.Content ?? "";

                // Build preview snippet (limit to 150 chars)
                string snippet = content.Length > 150
                    ? content.Substring(0, 150) + "..."
                    : content;

                // Add ranked result
                results.Add(new SearchResultDto
                {
                    Id = doc.Id,
                    Title = doc.Title,
                    ContentSnippet = snippet,
                    Score = Math.Round(score, 4)
                });
            }

            // Sort results by relevance score (highest first)
            return results.OrderByDescending(r => r.Score).ToList();
        }

    }
}
