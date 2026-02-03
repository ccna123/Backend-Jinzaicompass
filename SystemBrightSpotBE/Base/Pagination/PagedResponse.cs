namespace SystemBrightSpotBE.Base.Pagination
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public T Result { get; set; }
        public QueryParams? Query { get; set; }
        public PagedResponse(T result, int pageNumber, int pageSize, QueryParams? query)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.Result = result;
            this.Query = query;
        }
    }
}
