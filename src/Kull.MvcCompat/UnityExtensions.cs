using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Unity;
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

        public static IUnityContainer AddSingleton<T>(this IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof(T), new SingletonLifetimeManager());
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
