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
            unityContainer.RegisterType(typeof(T), GetRandomId(), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddTransient<T, T2>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), typeof(T2), GetRandomId(), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddTransient<T>(this IUnityContainer unityContainer,
            Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(GetRandomId(), c => func(GetIServiceProvider(c)), new TransientLifetimeManager());
            return unityContainer;
        }

        private class ServiceProviderUnity: IServiceProvider
        {
            private IUnityContainer unityContainer;
            public ServiceProviderUnity(IUnityContainer unityContainer)
            {
                this.unityContainer = unityContainer;
            }

            public object GetService(Type serviceType)
            {
                return this.unityContainer.Resolve(serviceType);
            }
        }
        private static IServiceProvider GetIServiceProvider(IUnityContainer c)
        {
            return new ServiceProviderUnity(c);
        }

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer,
            T instance)
        {
            unityContainer.RegisterFactory<T>(GetRandomId(),
                c => instance, new SingletonLifetimeManager());
            return unityContainer;
        }

        static int cnt = 0;
        private static string GetRandomId()
        {
            return "id_" + new Random().Next(0,100).ToString() + (++cnt);
        }

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer,
            Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(
                GetRandomId(),
                c => func(GetIServiceProvider(c)), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer,
           Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(
                 GetRandomId(),
                 c => func(GetIServiceProvider(c)), new PerThreadLifetimeManager());
            return unityContainer;
        }


        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), GetRandomId(), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddSingleton<T, T2>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), typeof(T2), GetRandomId(), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer)
        {
            // Seems to be the closest match
            unityContainer.RegisterType(typeof(T), GetRandomId(), new Unity.Lifetime.PerThreadLifetimeManager());
            return unityContainer;
        }
        public static IUnityContainer TryAddSingleton<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddSingleton<T>(this IUnityContainer unityContainer, T instance)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterFactory(typeof(T), c=>instance, new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddTransient<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddTransient<T, T2>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), typeof(T2), new TransientLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddScoped<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), new PerThreadLifetimeManager());
            return unityContainer;
        }

    }
}
