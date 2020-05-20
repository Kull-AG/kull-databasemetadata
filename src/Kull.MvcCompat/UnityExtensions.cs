using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Kull.MvcCompat
{
    public static class UnityExtensions
    {
        public static IUnityContainer AddTransient<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddTransient<T, T2>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), typeof(T2), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddTransient<T>(this IUnityContainer unityContainer,
            Func<IUnityContainer, T> func)
        {
            unityContainer.RegisterFactory<T>(c => func(c), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer,
            Func<IUnityContainer, T> func)
        {
            unityContainer.RegisterFactory<T>(c => func(c), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer,
           Func<IUnityContainer, T> func)
        {
            unityContainer.RegisterFactory<T>(c => func(c), new PerThreadLifetimeManager());
            return unityContainer;
        }


        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddSingleton<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddTransient<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddScoped<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new PerThreadLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer)
        {
            // Seems to be the closest match
            unityContainer.RegisterType(typeof(T), new Unity.Lifetime.PerThreadLifetimeManager());
            return unityContainer;
        }
    }
}
