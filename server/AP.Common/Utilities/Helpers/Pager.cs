namespace AP.Common.Utilities.Helpers;

public static class Pager
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;

    public static void Calculate(int count, int? page, int? size, /*out int? pageNum, out int? pageSize, */out int skipRows, out int takeRows)
    {
        if (page != null && size == null)
        {
            size = DefaultPageSize;
        }
        else if (page == null && size != null)
        {
            page = DefaultPageNumber;
        }

        ////pageNum = page;
        ////pageSize = size;

        skipRows = ((page ?? DefaultPageNumber) - 1) * (size ?? DefaultPageSize);
        takeRows = size != null ? (int)size : count;
        ////takeRows = size != null ? (int)size : page != null ? defaultPageSize : count;
    }
}