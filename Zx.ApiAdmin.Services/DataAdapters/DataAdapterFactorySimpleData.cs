using System;
using Zx.ApiAdmin.Utilities;
using Zx.ApiAdmin.Wrappers;

namespace Zx.ApiAdmin.Services.DataAdapters
{
    public class DataAdapterFactorySimpleData
    {
        /// <summary>
        /// 实现类的前部
        /// </summary>
        private const string ServiceInstanceFrontPath = "Zx.ApiAdmin.RepositorySimpleData";

        /// <summary>
        /// 实现类的程序集名称
        /// </summary>
        const string ServiceProgrammer = "Zx.ApiAdmin.RepositorySimpleData";

        /// <summary>
        /// 获取服务实现实例
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="asynchronous">是否为异步操作</param>
        /// <param name="hasParam">构造函数参数</param>
        /// <returns></returns>
        private static T GetDataAdapterPublic<T>(bool asynchronous, bool? hasParam)
        {
            Type interfaceType = typeof(T);

            ImplementionConfigration config = new ImplementionConfigration();

            config.InterfaceType = interfaceType;


            config.InstanceFrontPath = ServiceInstanceFrontPath;

            config.InstanceProgrammer = ServiceProgrammer;

            object serviceInstance = null;

            if (hasParam.HasValue)
            {
                serviceInstance = ProxyHelper.GetServiceInstance(config, hasParam.Value);
            }
            else
            {
                serviceInstance = ProxyHelper.GetServiceInstance(config);
            }

            var adapter = (T)serviceInstance;

            return adapter;
        }



        public static IApiDataAdapter GetApiDataAdapter(bool isRead = false)
        {
            return (IApiDataAdapter)GetDataAdapterPublic<IApiDataAdapter>(false, isRead);
        }
    }
}
