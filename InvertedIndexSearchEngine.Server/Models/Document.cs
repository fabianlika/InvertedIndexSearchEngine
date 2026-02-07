using System;
using System.Collections.Generic;

namespace InvertedIndexSearchEngine.Server.Models;

public partial class Document
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public virtual ICollection<InvertedIndex> InvertedIndices { get; set; } = new List<InvertedIndex>();
}
