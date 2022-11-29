using System;
using Unity;

namespace Kull.MvcCompat
{

    internal class ServiceProviderUnity : IServiceProvider, IServiceScopeFactory
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

        public IServiceScope CreateScope()
        {
            return new UnityScope(this.unityContainer.CreateChildContainer());
        }
    }
    internal class UnityScope : IServiceScope
    {
        private IUnityContainer unityContainer;
        public UnityScope(IUnityContainer unityContainer)
        {
            this.unityContainer = unityContainer;
            this.ServiceProvider = new ServiceProviderUnity(unityContainer);
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public void Dispose() => unityContainer.Dispose();
    }
}

