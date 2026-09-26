namespace FlipLeo.Repository.Entities;

public class LookupFlipStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<FlipRecord> FlipRecords { get; set; } = new List<FlipRecord>();
}
