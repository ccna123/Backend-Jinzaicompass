using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Filters;

namespace SystemBrightSpotBE.Helpers
{
    public class PaginationHelper
    {
        public static PagedResponse<List<T>> CreatePagedResponse<T>(List<T> pagedData, PaginationFilter validFilter, int totalRecords, QueryParams? query)
        {
            var response = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize, query);
            var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
            response.TotalPages = roundedTotalPages;
            response.TotalRecords = totalRecords;
            return response;
        }
    }
}
