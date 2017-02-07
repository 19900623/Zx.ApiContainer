using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;

namespace Zx.ApiAdmin.Wrappers
{
    public class ServiceFactoryTinyIoC
    {
        private static readonly TinyIoCContainer Ioc = new TinyIoCContainer();

        static ServiceFactoryTinyIoC()
        {
            Ioc.Register(typeof(IApiContainerService), "ApiContainerServiceSimpleData").AsSingleton();
            //...
        }

        public static T GetService<T>(bool isRead = false) where T : class
        {
            return Ioc.Resolve<T>();
        }
    }
}
