# if NETFX 
using Unity;
using Unity.Lifetime;
using Kull.MvcCompat;
using IServiceCollection = Unity.IUnityContainer;
#else
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Generic;
using System.Text;

namespace Kull.DatabaseMetadata
{
    public static class DependencyInjection
    {
        public static void AddKullDatabaseMetadata(this IServiceCollection services)
        {
            services.AddTransient<SqlHelper>();
            services.AddSingleton<SPParametersProvider>();

        }
    }

}