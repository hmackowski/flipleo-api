namespace FlipLeo.Core.Constants;

/// <summary>Ids of the rows in dbo.LookupFlipStatus (seeded by PostDeployment\Populate_LookupFlipStatus.sql).</summary>
public static class FlipStatusIds
{
    public const int Bought = 1;
    public const int Listed = 2;
    public const int Sold = 3;
}
