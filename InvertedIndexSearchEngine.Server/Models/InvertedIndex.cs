using System;
using System.Collections.Generic;

namespace InvertedIndexSearchEngine.Server.Models;

public partial class InvertedIndex
{
    public int TermId { get; set; }

    public int DocumentId { get; set; }

    public int? Frequency { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Term Term { get; set; } = null!;
}
