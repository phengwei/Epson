using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Extensions
{
    public class ScopedTaskRunner
    {
        private readonly IServiceProvider _serviceProvider;

        public ScopedTaskRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RunInScope(Action<IServiceProvider> task)
        {
            Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;
                    task(scopedProvider);
                }
            });
        }
    }

}
