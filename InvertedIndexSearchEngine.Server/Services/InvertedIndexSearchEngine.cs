using DbDocument = InvertedIndexSearchEngine.Server.Models.Document;
using InvertedIndexSearchEngine.Server.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using System.Text.RegularExpressions;

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
            // 1️⃣ Save Document
            var doc = new DbDocument
            {
                Title = title,
                Content = content
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            // 2️⃣ MAP Phase — Tokenize & Normalize
            var words = Regex.Replace(content.ToLower(), @"[^\w\s]", "")
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w))
                .ToList();

            // 3️⃣ REDUCE Phase — Count Term Frequencies
            var wordCounts = words
                .GroupBy(w => w)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .ToList();

            foreach (var item in wordCounts)
            {
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.Word == item.Word);

                if (term == null)
                {
                    term = new Term { Word = item.Word };
                    _context.Terms.Add(term);
                    await _context.SaveChangesAsync();
                }

                _context.InvertedIndices.Add(new InvertedIndex
                {
                    DocumentId = doc.Id,
                    TermId = term.Id,
                    Frequency = item.Count
                });
            }

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
    }
}
