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

namespace Kull.DatabaseMetadata;

public static class DependencyInjection
{
    /// <summary>
    /// Adds metadata to your services, namely SqlHelper, DBObjects, Keys and SPParametersProvider
    /// </summary>
    /// <param name="services">The services</param>
    /// <returns></returns>
    public static IServiceCollection AddKullDatabaseMetadata(this IServiceCollection services)
    {
        services.TryAddSingleton<ISPParameterProviderCache>(new SPParameterProviderMemoryCache());
        services.AddTransient<SqlHelper>();
        services.AddTransient<DBObjects>();
        services.AddTransient<Keys>();
        services.AddTransient<SPParametersProvider>();
        return services;
    }
}
