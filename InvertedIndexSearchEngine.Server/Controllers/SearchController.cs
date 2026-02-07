using InvertedIndexSearchEngine.Server.DTOs;
using InvertedIndexSearchEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvertedIndexSearchEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IndexerService _indexer;
        private readonly SearchService _searchService;

        public SearchController(IndexerService indexer, SearchService searchService)
        {
            _indexer = indexer;
            _searchService = searchService;
        }

        /// <summary>
        /// GET api/search/search?q=term
        /// Searches indexed documents using TF-IDF and returns ranked results.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var results = await _searchService.SearchAsync(q);
            return Ok(results);
        }

        /// <summary>
        /// POST api/search/seed
        /// Seeds sample documents and indexes them.
        /// </summary>
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            await _indexer.AddDocumentAndIndex(
                "Database Systems",
                "An inverted index maps words to documents to enable fast search.");

            await _indexer.AddDocumentAndIndex(
                "Information Retrieval",
                "TF-IDF evaluates word importance across documents.");

            await _indexer.AddDocumentAndIndex(
                "Search Engines",
                "Search engines use inverted indexes to enable fast full-text search.");

            return Ok("Seeded successfully.");
        }


        /// <summary>
        /// Upload a new document with plain text.
        /// Saves to DB and indexes terms for search.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] UploadDocumentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Title and content are required.");

            await _indexer.AddDocumentAndIndex(dto.Title, dto.Content);

            return Ok("Document uploaded and indexed successfully.");
        }

        /// <summary>
        /// POST api/search/upload-file
        /// Upload file (PDF, DOCX, TXT). Extracts text and indexes it.
        /// </summary>
        [HttpPost("upload-file")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadFile([FromForm] string title, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(title))
                title = Path.GetFileNameWithoutExtension(file.FileName);

            try
            {
                await _indexer.AddDocumentFromFile(title, file);
                return Ok("File uploaded and indexed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"File processing failed: {ex.Message}");
            }
        }
    }
}
