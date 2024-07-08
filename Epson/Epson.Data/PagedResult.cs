using Epson.Core.Domain.Base;

namespace Epson.Data
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int Total { get; set; }
    }
}
