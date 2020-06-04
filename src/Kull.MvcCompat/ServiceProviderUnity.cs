using System;
using Unity;

namespace Kull.MvcCompat
{

    internal class ServiceProviderUnity : IServiceProvider
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

}

