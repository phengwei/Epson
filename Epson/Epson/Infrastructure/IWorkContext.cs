using Epson.Model.Users;

namespace Epson.Infrastructure
{
    public interface IWorkContext
    {
        Customer CurrentUser { get; set; }
    }
}
