using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;

namespace Zx.ApiAdmin.Services.DataAdapters
{
    public class DataAdapterFactorySimpleDataTinyIoC
    {
        private static readonly TinyIoCContainer Ioc = new TinyIoCContainer();

        static DataAdapterFactorySimpleDataTinyIoC()
        {
            Ioc.Register(typeof(IApiContainerAdpt), "ApiDataAdapter").AsSingleton();
            //...
        }

        public static T GetDataAdapter<T>(bool isRead = false) where T:class
        {
            return Ioc.Resolve<T>();
        }
    }
}
