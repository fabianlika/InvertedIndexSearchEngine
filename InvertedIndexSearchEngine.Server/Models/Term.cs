using System;
using System.Collections.Generic;

namespace InvertedIndexSearchEngine.Server.Models;

public partial class Term
{
    public int Id { get; set; }

    public string? Word { get; set; }

    public virtual ICollection<InvertedIndex> InvertedIndices { get; set; } = new List<InvertedIndex>();
}
