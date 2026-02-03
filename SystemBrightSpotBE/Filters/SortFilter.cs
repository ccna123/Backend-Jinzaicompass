namespace SystemBrightSpotBE.Filters
{
    public class SortFilter
    {
        public string? SortBy;
        public string? SortColumn;

        public SortFilter(string? sortBy, string? sortColumn)
        {
            SortBy = sortBy != "desc" && sortBy != "asc" && sortBy == null ? "asc" : sortBy;
            SortColumn = sortColumn;
        }
    }
}
