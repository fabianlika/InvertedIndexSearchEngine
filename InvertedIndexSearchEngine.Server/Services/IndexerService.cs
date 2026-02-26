using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InvertedIndexSearchEngine.Server.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using DbDocument = InvertedIndexSearchEngine.Server.Models.Document;


namespace InvertedIndexSearchEngine.Services
{
    public class IndexerService
    {
        private readonly SearchDbContext _context;

        private readonly HashSet<string> _stopWords = new()
        {
            "the","is","at","which","on","and","a","an","to","of","in","it","for","with","by",
            "është","dhe","në","me","për","nga","si","kjo","ajo"
        };

        public IndexerService(SearchDbContext context)
        {
            _context = context;
        }

        public async Task AddDocumentAndIndex(string title, string content)
        {
            // ============================
            // 1️⃣ SAVE DOCUMENT TO DATABASE
            // ============================
            // Store raw document text before indexing
            var doc = new DbDocument
            {
                Title = title,
                Content = content
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync(); // Save to generate Document ID


            // ==========================================
            // 2️⃣ MAP PHASE — TOKENIZE & NORMALIZE TEXT
            // ==========================================
            // Map step transforms raw text into normalized tokens (words)

            var words = Regex.Replace(content.ToLower(), @"[^\w\s]", "") // Remove punctuation
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries) // Split text into words
                .Where(w => !_stopWords.Contains(w)) // Remove stopwords (e.g., "the", "and", "is")
                .ToList();

            // Example output of MAP:
            // Input: "Search engines index documents"
            // Output: ["search", "engines", "index", "documents"]


            // =====================================================
            // 3️⃣ REDUCE PHASE — COUNT TERM FREQUENCY PER DOCUMENT
            // =====================================================
            // Reduce step aggregates mapped tokens into (term → frequency)

            var wordCounts = words
                .GroupBy(w => w) // Group same words
                .Select(g => new { Word = g.Key, Count = g.Count() }) // Count occurrences
                .ToList();

            // Example output of REDUCE:
            // "index" → 3 times
            // "search" → 1 time


            // =====================================================
            // 4️⃣ BUILD INVERTED INDEX ENTRIES
            // =====================================================
            foreach (var item in wordCounts)
            {
                // Check if term already exists in Terms table
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.Word == item.Word);

                // If term does not exist, create it
                if (term == null)
                {
                    term = new Term { Word = item.Word };
                    _context.Terms.Add(term);
                    await _context.SaveChangesAsync(); // Save to generate Term ID
                }

                // Insert entry into Inverted Index table:
                // Maps Term → Document + Frequency
                _context.InvertedIndices.Add(new InvertedIndex
                {
                    DocumentId = doc.Id,
                    TermId = term.Id,
                    Frequency = item.Count // TF = Term Frequency
                });
            }

            // Persist all inverted index entries
            await _context.SaveChangesAsync();
        }


        public async Task AddDocumentFromFile(string title, IFormFile file)
        {
            string content = await ExtractTextFromFile(file);

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("No readable text extracted from file.");

            await AddDocumentAndIndex(title, content);
        }

        private async Task<string> ExtractTextFromFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            // TXT
            if (ext == ".txt")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                return await reader.ReadToEndAsync();
            }

            // DOCX — OpenXML
            if (ext == ".docx")
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                using var wordDoc = WordprocessingDocument.Open(ms, false);
                var body = wordDoc.MainDocumentPart?.Document.Body;

                if (body == null)
                    return string.Empty;

                var sb = new StringBuilder();

                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    sb.AppendLine(paragraph.InnerText);
                }

                return sb.ToString();
            }

            // PDF — iText7
            if (ext == ".pdf")
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                using var reader = new PdfReader(ms);
                using var pdf = new PdfDocument(reader);

                var sb = new StringBuilder();

                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                {
                    sb.Append(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        public async Task<InvertedIndexSearchEngine.Server.Models.Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

    }
}
