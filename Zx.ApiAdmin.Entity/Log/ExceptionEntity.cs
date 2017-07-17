using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zx.ApiAdmin.Entity.Log
{
    /// <summary>
    /// 异常实体
    /// </summary>
    [Serializable]
    public class ExceptionEntity
    {
        /// <summary>
        /// 当前异常
        /// </summary>
        public Exception CurrentException
        {
            get;
            set;
        }

        /// <summary>
        /// 异常简历机器
        /// </summary>
        public string HostName
        {
            get;
            set;
        }

    }
}
