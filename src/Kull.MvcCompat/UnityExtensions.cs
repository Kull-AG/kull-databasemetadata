﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace Kull.MvcCompat
{
    /// <summary>
    /// Adds methods that allow the use of extension methods which are similar to .net core integrated container. 
    /// The goal is to have the same behavior as those
    /// </summary>
    public static partial class UnityExtensions
    {
        public static Func<ITypeLifetimeManager> createTransient = () => new PerRequestLifetimeManager();
        public static Func<ITypeLifetimeManager> createScoped = () => new PerRequestLifetimeManager();

        public static IUnityContainer AddTransient<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), GetRandomId<T>(unityContainer), createTransient());
            return unityContainer;
        }

        public static IUnityContainer AddTransient<T, T2>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), typeof(T2), GetRandomId<T>(unityContainer), createTransient());
            return unityContainer;
        }

        public static IServiceScope CreateScope(this IUnityContainer serviceProvider)
        {
            return new UnityScope(serviceProvider);
        }


        public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException("Cannot find " + typeof(T).FullName);
        }

        public static IServiceScope CreateScope(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public static IUnityContainer AddTransient<T>(this IUnityContainer unityContainer,
            Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(GetRandomId<T>(unityContainer), c => func(GetIServiceProvider(c)), (IFactoryLifetimeManager)createTransient());
            return unityContainer;
        }
        private static IServiceProvider GetIServiceProvider(IUnityContainer c)
        {
            return new ServiceProviderUnity(c);
        }

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer,
            T instance)
        {
            unityContainer.RegisterFactory<T>(GetRandomId<T>(unityContainer),
                c => instance, new SingletonLifetimeManager());
            return unityContainer;
        }

        static int cnt = 0;
        private static string? GetRandomId<T>(IUnityContainer container)
        {
            if (container.IsRegistered<T>()) return "id_" + new Random().Next(0, 100).ToString() + (++cnt);
            return null;
        }

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer,
            Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(
                GetRandomId<T>(unityContainer),
                c => func(GetIServiceProvider(c)), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer,
           Func<IServiceProvider, T> func)
        {
            unityContainer.RegisterFactory<T>(
                 GetRandomId<T>(unityContainer),
                 c => func(GetIServiceProvider(c)), (IFactoryLifetimeManager)createScoped());
            return unityContainer;
        }


        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), GetRandomId<T>(unityContainer), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddSingleton<T, T2>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), typeof(T2), GetRandomId<T>(unityContainer), new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer AddScoped<T>(this IUnityContainer unityContainer)
        {
            // Seems to be the closest match
            unityContainer.RegisterType(typeof(T), GetRandomId<T>(unityContainer), createScoped());
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
            unityContainer.RegisterFactory(typeof(T), c => instance, new SingletonLifetimeManager());
            return unityContainer;
        }

        public static IUnityContainer TryAddSingleton<T, T2>(this IUnityContainer unityContainer, T instance)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), typeof(T2), new SingletonLifetimeManager());
            return unityContainer;
        }


        public static IUnityContainer TryAddTransient<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), createTransient());
            return unityContainer;
        }

        public static IUnityContainer TryAddTransient<T, T2>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), typeof(T2), createTransient());
            return unityContainer;
        }

        public static IUnityContainer TryAddScoped<T>(this IUnityContainer unityContainer)
        {
            if (unityContainer.IsRegistered(typeof(T))) return unityContainer;
            unityContainer.RegisterType(typeof(T), createScoped());
            return unityContainer;
        }

    }
}
