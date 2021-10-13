# if NETFX 
using Unity;
using Unity.Lifetime;
using Kull.MvcCompat;
using IServiceCollection = Unity.IUnityContainer;
#else
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.TryAddSingleton<ISPParameterProviderCache>(new SPParameterProviderMemoryCache());
            services.AddTransient<SqlHelper>();
            services.AddTransient<DBObjects>();
            services.AddTransient<Keys>();
            services.AddTransient<SPParametersProvider>();

        }
    }

}