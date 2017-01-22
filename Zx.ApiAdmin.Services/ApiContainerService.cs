using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Zx.ApiAdmin.Entity.Admin;
using Zx.ApiAdmin.Services.DataAdapters;
using Zx.ApiAdmin.Utilities;
using Zx.ApiAdmin.Wrappers;
using System.Net;

namespace Zx.ApiAdmin.Services
{
    public class ApiContainerService : IApiContainerService
    {
        /// <summary>
        /// 获取数据访问适配器
        /// </summary>
        /// <returns></returns>
        private IApiDataAdapter GetAdapter()
        {
            return DataAdapterFactory.GetApiDataAdapter();
        }

        private IApiDataAdapter GetAdapterSimpleData()
        {
            //尝试用mef
            return DataAdapterFactory.GetApiDataAdapter();
        }

        /// <summary>
        /// 获取api现有站点
        /// </summary>
        /// <returns></returns>
        public List<ApiContainerSiteConfig> GetApiContainerSites()
        {
            var list = GetAllSite();
            return list;
        }

        /// <summary>
        /// 获取最新更新记录
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public ApiContainerUploadRecord GetApiContainerLastUploadRecord(int siteId)
        {
            var list = GetUploadRecordsBySiteId(siteId);
            var record = list.FirstOrDefault();
            return record;
        }

        /// <summary>
        /// 根据siteId获取api站点的路由映射
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<ApiContainerRouteMapping> GetApiContainerRouteMappingBySite(int siteId)
        {
            var list = GetRouteMappingsBySiteId(siteId);
            return list;
        }

        /// <summary>
        /// 获取指定站点更新记录
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<ApiContainerUploadRecord> GetApiContainerUploadRecord(int siteId)
        {
            var list = GetUploadRecordsBySiteId(siteId);
            return list;
        }

        /// <summary>
        /// 获取指定更新记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ApiContainerUploadRecord GetApiContainerUploadRecordById(int id)
        {
            var record = GetUploadRecordById(id);
            return record;
        }

        /// <summary>
        /// 获取api站点LibPath
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private List<string> GetApiLibPath(int siteId)
        {
            var site = GetSiteById(siteId);
            if(site==null)return new List<string>();
            var strMachines = site.Machine ?? string.Empty;
            var liTarget =
                strMachines.Split(',')
                    .Select(m => string.Format(@"\\{0}\{1}\", m, site.LibFolder ?? string.Empty))
                    .ToList();
            return liTarget;
        }

        /// <summary>
        /// 根据site和更新记录获取ApiContainerRouteMappingHistory
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        private IEnumerable<ApiContainerRouteMappingHistory> GetApiContainerRouteMappingHistoryBySiteAndUploadRecord(int siteId, int recordId)
        {
            var list = GetSiteRouteMappingHistoryByRecordId(siteId, recordId);
            return list;
        }

        /// <summary>
        /// 同步RouteMappings
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private bool SyncApiContainerRouteMappings(int siteId, List<ApiContainerRouteMapping> mappings)
        {
            try
            {
                DeleteSiteRouteMapping(siteId);
            }
            catch (Exception e)
            {
                return false;
            }
            var res = true;
            mappings.ForEach(m =>
            {
                var dbm = InsertSiteRouteMapping(m);
                res = res && dbm != null;
            });
            return res;
        }

        /// <summary>
        /// 添加上传记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private int RecordUploadApiContainer(ApiContainerUploadRecord record)
        {
            var dbRecord = InsertUploadRecord(record);
            if (dbRecord == null)
                throw new Exception("RecordUploadApiContainer失败");
            return dbRecord.ID;
        }

        /// <summary>
        /// 同步lib目录
        /// </summary>
        /// <param name="sourceLibPath"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public bool SyncLib(string sourceLibPath, int siteId)
        {
            var source = sourceLibPath ?? string.Empty;
            var target = GetApiLibPath(siteId);
            try
            {
                target.ForEach(t => FileHelper.DirectoryCopy(source, t));
                //FileHelper.DirectoryCopy(source, target);
            }
            catch (Exception e)
            {
                var tile = string.Format("【RecycleAppPool异常】 {0} {1}", source, string.Join(",",target));
                LogHelper.LogException(new Exception(tile, e));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 同步db
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="remark"></param>
        /// <param name="backUpFolderName"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public bool SyncDb(int siteId, string remark, string backUpFolderName, List<ApiContainerRouteMapping> mappings)
        {
            try
            {
                if (!ApiContainerUpLoad(siteId, remark ?? string.Empty, backUpFolderName, mappings))
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 备份
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="tmpLibPath"></param>
        /// <param name="backupFullPath"></param>
        /// <returns></returns>
        public bool BackUp(int siteId, string tmpLibPath, string backupFullPath)
        {
            try
            {
                if (!FileHelper.CreateFolderIfNeeded(backupFullPath))
                    return false;
                FileHelper.DirectoryCopy(tmpLibPath ?? string.Empty, backupFullPath);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 回滚Mapping
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public bool RollbackApiContainerRouteMapping(int siteId, int recordId)
        {
            var history = GetApiContainerRouteMappingHistoryBySiteAndUploadRecord(siteId, recordId);
            var mappings = history.Select(h => new ApiContainerRouteMapping
            {
                Verb = h.Verb,
                Path = h.Path,
                ServiceAssembly = h.ServiceAssembly,
                ServiceName = h.ServiceName,
                MethodName = h.MethodName,
                SiteId = h.SiteId,
                IsAsyncInvoke = h.IsAsyncInvoke,
            }).ToList();
            if (!SyncApiContainerRouteMappings(siteId, mappings)) return false;
            var hRecord = GetApiContainerUploadRecordById(recordId);
            var historyTime = hRecord.AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            var remark = "回滚到:" + historyTime;
            var record = new ApiContainerUploadRecord
            {
                AddTime = DateTime.Now,
                SiteId = siteId,
                Remark = remark,
                BackupFolder = hRecord.BackupFolder ?? string.Empty
            };
            try
            {
                RecordUploadApiContainer(record);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取程序集中的类名
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <returns></returns>
        public List<string> GetAssembliesClassNames(string assemblyFile)
        {
            var loader = new AssemblyDynamicLoader();
            var names = loader.GetExportedTypeNames(assemblyFile);
            var liFullName = names.ToList();
            return liFullName;
        }

        /// <summary>
        /// 获取指定程序集类中的方法
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="classFullName"></param>
        /// <returns></returns>
        public List<ApiContainerMethodInfo> GetClassMethodNames(string assemblyFile, string classFullName)
        {
            var loader = new AssemblyDynamicLoader();
            const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static |
                                       BindingFlags.Instance;
            var methods = loader.GetMethods(assemblyFile, classFullName, flags);
            if (methods == null) return new List<ApiContainerMethodInfo>();
            var res = methods.Select(m => new ApiContainerMethodInfo
            {
                Name = m.Name,
                IsStatic = m.IsStatic,
                ReturnTypeName = m.ReturnTypeName,
                ReturnTypeFullName = m.ReturnTypeFullName,
                ParmTypes = m.ParmTypes
            }).ToList();
            return res;
        }

        /// <summary>
        /// 获取备份文件夹名
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public string GetBackupFolderName(int siteId)
        {
            var name = string.Format("site{0}_{1}", siteId, DateTime.Now.ToString("yyyyMMddHHmmss"));
            return name;
        }

        /// <summary>
        /// ApiContainerUpLoad更新数据
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="remark"></param>
        /// <param name="folderName"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public bool ApiContainerUpLoad(int siteId, string remark, string folderName, List<ApiContainerRouteMapping> mappings)
        {
            var faultCount = mappings.Count(m => !m.SiteId.Equals(siteId));
            if (faultCount > 0) return false;
            if (!SyncApiContainerRouteMappings(siteId, mappings)) return false;
            var record = new ApiContainerUploadRecord
            {
                AddTime = DateTime.Now,
                SiteId = siteId,
                Remark = remark,
                BackupFolder = folderName
            };
            int recordId;
            try
            {
                recordId = RecordUploadApiContainer(record);
            }
            catch (Exception e)
            {
                return false;
            }
            var mappingsHistory = mappings.Select(m => new ApiContainerRouteMappingHistory
            {
                Verb = m.Verb,
                Path = m.Path,
                ServiceAssembly = m.ServiceAssembly,
                ServiceName = m.ServiceName,
                MethodName = m.MethodName,
                SiteId = m.SiteId,
                IsAsyncInvoke = m.IsAsyncInvoke,
                UploadRecordId = recordId
            }).ToList();
            var saveRes = SaveApiContainerRouteMappingHistory(mappingsHistory);
            return saveRes;
        }

        /// <summary>
        /// 保存mapping到历史表
        /// </summary>
        /// <param name="mappingsHistory"></param>
        /// <returns></returns>
        private bool SaveApiContainerRouteMappingHistory(List<ApiContainerRouteMappingHistory> mappingsHistory)
        {
            var res = true;
            mappingsHistory.ForEach(mh =>
            {
                var mhdb = InsertRouteMappingHistory(mh);
                res = res && mhdb != null;
            });
            return res;
        }

        /// <summary>
        /// 回收应用程序池
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public bool RecycleAppPool(int siteId)
        {         
            var site = GetSiteById(siteId);
            if (site == null) return false;
            if (string.IsNullOrEmpty(site.ApiDomain)) return false;
            var appPool = site.ApiDomain.Replace("http://", "");
            var res = true;
            site.Machine.Split(',').ForEach(m =>
            {
                var doRes = DoRecycle(m, appPool);
                res = res && doRes;
            });
            return res;
        }

        #region WMI

        private bool DoRecycle1(string machine, string appPool)
        {           
            var commandLine = string.Format(@"c:\windows\system32\inetsrv\appcmd stop apppool {0}", appPool);
            try
            {               
                var options = new ConnectionOptions
                {
                    EnablePrivileges = true,
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.PacketPrivacy
                };                
                var strPath = @"root\cimv2:Win32_Process";
                if (!IsLocal(machine))
                {
                    options.Username = @"highpin\zhaoxu";
                    options.Password = "zhuopin";
                    strPath = string.Format(@"\\{0}\root\cimv2:Win32_Process", machine);
                }               
                var path = new ManagementPath(strPath);
                var scope = new ManagementScope(path, options);
                scope.Connect();
                var opt = new ObjectGetOptions();
                var classInstance = new ManagementClass(scope, path, opt);
                var inParams = classInstance.GetMethodParameters("Create");
                inParams["CommandLine"] = commandLine;
                var outParams = classInstance.InvokeMethod("Create", inParams, null);
                if (outParams == null) return false;
                var retVal = (outParams["returnValue"] ?? 0).ToString();
                //LogHelper.LogException(new Exception("回收" + machine + " " + strPath));
                return retVal.Equals("0");
            }
            catch (Exception e)
            {
                var tile = string.Format("【RecycleAppPool异常】 {0} {1}", machine, appPool);
                LogHelper.LogException(new Exception(tile, e));
                return false;
            }        
        }

        private bool IsLocal(string ip)
        {
            if ("127.0.0.1".Equals(ip))
                return true;
            var ipHost = Dns.GetHostAddresses(Dns.GetHostName());            
            return ipHost.Select(add => add.ToString()).Contains(ip);
        }

        public void IisTest()
        {

            try
            {
                //var appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
                //var appPools = new DirectoryEntry("IIS://192.168.201.121/W3SVC/AppPools", "highpin\\zhaoxu", "zhuopin");
                //var appPools = new DirectoryEntry("IIS://192.168.201.121/W3SVC/AppPools", "highpin\\builduser", "zhuopin");
                var appPools = new DirectoryEntry("IIS://192.168.66.241/W3SVC/AppPools", "zhaoxu", "zhaopin@123");
                //var poolName = "api.highpin.cn";
                var poolName = "m.highpin.cn";
                var appPool = appPools.Children.Find(poolName, "IIsApplicationPool");
                var methodName = "Recycle"; //Start启动，Stop停止，Recycle回收
                appPool.Invoke(methodName, null);
                appPools.CommitChanges();
                appPools.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                Console.ReadLine();
            }
        }

        public void IisTest1()
        {
            //System.Management.ConnectionOptions
            try
            {
                //var connectionOptions = new ConnectionOptions { Username = "highpin\\zhaoxu", Password = "zhuopin" };
                //var managementScope = new ManagementScope(@"\\localhost\root\microsoftiisv2", connectionOptions);
                //managementScope.Connect();


                var IIsApplicationPool = "m.highpin.cn";
                //ManagementScope scope = new ManagementScope(@"\\localhost\root\MicrosoftIISv2");
                //ManagementScope scope = new ManagementScope(@"\\192.168.201.121\root\MicrosoftIISv2",
                //    new ConnectionOptions
                //    {
                //        Username = "administrator",
                //        Password = "zhaopin@123",
                //        Impersonation = ImpersonationLevel.Default
                //    });
                //ManagementScope scope = new ManagementScope(@"\\192.168.201.121\root\MicrosoftIISv2",
                //    new ConnectionOptions
                //    {
                //        Username = "highpin\\zhaoxu",
                //        Password = "zhuopin",
                //        Impersonation = ImpersonationLevel.Anonymous
                //    });

                //ManagementScope scope = new ManagementScope(@"\\192.168.66.241\root\MicrosoftIISv2",
                //    new ConnectionOptions
                //    {
                //        Username = "zhaoxu",
                //        Password = "zhaopin@123",
                //        //Impersonation = ImpersonationLevel.Default,
                //        EnablePrivileges = true,
                //        Authentication = System.Management.AuthenticationLevel.PacketPrivacy
                //    });
                //scope.Connect();
                //ManagementObject appPool = new ManagementObject(scope, new ManagementPath("IIsApplicationPool.Name='W3SVC/AppPools/" + IIsApplicationPool + "'"), null);

                ManagementScope scope = new ManagementScope(@"\\192.168.66.241\root\webAdministration",
                    new ConnectionOptions
                    {
                        Username = "zhaoxu",
                        Password = "zhaopin@123",
                        Impersonation = ImpersonationLevel.Impersonate,
                        //EnablePrivileges = true,
                        Authentication = System.Management.AuthenticationLevel.PacketPrivacy
                    });
                scope.Connect();
                ManagementObject appPool = new ManagementObject(scope, new ManagementPath("ApplicationPool.Name='" + IIsApplicationPool + "'"), null);

                //ManagementScope scope = new ManagementScope(@"\\192.168.66.126\root\webAdministration",null);
                //scope.Connect();
                //ManagementObject appPool = new ManagementObject(scope, new ManagementPath("ApplicationPool.Name='" + IIsApplicationPool + "'"), null);

                appPool.InvokeMethod("Recycle", null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }            
        }

        public void IisTest4()
        {
            try
            {
                var path = new ManagementPath(@"\\192.168.66.241\root\cimv2:Win32_Process");
                var options = new ConnectionOptions
                {
                    Username = "zhaoxu",
                    Password = "zhaopin@123",
                    EnablePrivileges = true,
                    Authentication = AuthenticationLevel.PacketPrivacy
                };
                var scope = new ManagementScope(path, options);
                scope.Connect();
                var opt = new ObjectGetOptions();
                var classInstance = new ManagementClass(scope, path, opt);
                var inParams = classInstance.GetMethodParameters("Create");
                //inParams["CommandLine"] = @"c:\windows\system32\inetsrv\appcmd recycle apppool m.highpin.cn";
                inParams["CommandLine"] = @"c:\windows\system32\inetsrv\appcmd start apppool 'm.highpin.cn'";
                var outParams = classInstance.InvokeMethod("Create", inParams, null);
                var retStr = string.Format("returnValue:{0};processId:{1}", outParams["returnValue"],
                    outParams["processId"]);
                Console.WriteLine(retStr);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }            
        }

        public void IisTest2()
        {
            string strPath = @"\\192.168.201.132\root\cimv2:Win32_Service";
            System.Management.ManagementClass managementClass = new System.Management.ManagementClass(strPath);

            System.Management.ManagementScope managementScope = null;
            System.Management.ConnectionOptions connectionOptions = new System.Management.ConnectionOptions();
            connectionOptions.Username = "highpin\builduser";
            connectionOptions.Password = "zhuopin";
            managementScope = new System.Management.ManagementScope(@"\\192.168.201.132\root\cimv2", connectionOptions);
            managementClass.Scope = managementScope;

            System.Management.ManagementObject mo = managementClass.CreateInstance();
            if (mo != null)
            {
                mo.Path = new System.Management.ManagementPath(strPath + ".Name=\"HighEndWinService\"");
                try
                {
                    //判断是否可以停止
                    if ((bool)mo["AcceptStop"]) //(string)mo["State"]=="Running"
                    {
                        mo.InvokeMethod("StopService", null);

                        
                    }
                }
                catch (System.Management.ManagementException)
                {
                    
                }
            }

            
        }

//        public void IisTest3()
//        {
//            string command = String.Empty;//string variable..
////Passing WMIC code to command variable.For recycling more than one apppool declare multiple string variables and pass WMIC code like mentioned below.
 
//command = "wmic /namespace:" + "\"\\\\root/MicrosoftIISv2\"" + " /node:" + " \"Your_Server_Name\"  " + "path IISApplicationPool where (name like '%Your_IISAppool1%') call recycle";
//           ExecuteCmd cmd = new ExecuteCmd();//declaring object of class ExecuteCmd
//           cmd.ExecuteCommandAsync(command);//Passing string variable containing WMIC code to method ExecuteCommandAsync declared in class ExecuteCmd.
 
////This"ExecuteCommandAsync" method is used  to execute the WMIC Command. it is in the class "ExecuteCmd" 

//            try
//            {
//                //Asynchronously start the Thread to process the Execute command request.
//                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
//                //Make the thread as background thread.
//                objThread.IsBackground = true;
//                //Set the Priority of the thread.
//                objThread.Priority = ThreadPriority.AboveNormal;
//                //Start the thread.
//                objThread.Start(command);
//            }
//            catch (ThreadStartException objException)
//            {
//                // Log the exception
//            }
//            catch (ThreadAbortException objException)
//            {
//                // Log the exception

//            }
//            catch (Exception objException)
//            {
//                // Log the exception
//                //MessageBox.Show(objException.Message.ToString());
//            }
//        }

        public void IpTest()
        {
            StringBuilder Info = new StringBuilder("");
             IPAddress[] ipHost = Dns.GetHostAddresses(Dns.GetHostName());
             Info.Append("本机名：");
             Info.Append(Dns.GetHostName());
             Info.Append(" -> ");
             Info.Append("IP 地址：");
             Info.Append(" -> ");
             foreach (IPAddress address in ipHost)
             {
                 Info.Append(address.ToString());
                 Info.Append(" >> ");
             }
        }

        public bool DoRecycle(string machine, string appPool)
        {
            bool result;
            try
            {
                var co = new ConnectionOptions {Authentication = AuthenticationLevel.PacketPrivacy};
                if (!IsLocal(machine))
                {
                    //co.Username = @"highpin\zhaoxu";
                    //co.Password = "zhuopin";
                    co.Username = @"corp\zhaoxu";
                    co.Password = "Asd!@#222";
                }
                var ms = new ManagementScope("\\\\" + machine + "\\root\\microsoftiisv2", co);
                ms.Connect();
                var queryString = "Select * from IIsApplicationPool where Name='W3SVC/APPPOOLS/" + appPool + "'";
                var oq = new ObjectQuery(queryString);
                var query = new ManagementObjectSearcher(ms, oq);
                var queryCollection = query.Get();
                using (var enumerator = queryCollection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var mo = (ManagementObject)enumerator.Current;
                        //mo.InvokeMethod("Recycle", null);
                        mo.InvokeMethod("Stop", null);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                var tile = string.Format("【RecycleAppPool异常】 {0} {1}", machine, appPool);
                LogHelper.LogException(new Exception(tile, ex));
                result = false;
            }
            return result;
        }

        #endregion

        #region repos

        public List<ApiContainerSiteConfig> GetAllSite()
        {
            var adpt = GetAdapter();
            var list = adpt.Query<ApiContainerSiteConfig>(where: null, paramDic: null);
            return list;
        }

        public ApiContainerSiteConfig GetSiteById(int id)
        {
            const string @where = "ID=@siteId";
            var paramDic = new Dictionary<string, object> { { "@siteId", id } };
            var adpt = GetAdapter();
            var list = adpt.Query<ApiContainerSiteConfig>(where, paramDic);
            if (list == null) return null;
            var site = list.FirstOrDefault();
            return site;
        }

        public List<ApiContainerUploadRecord> GetUploadRecordsBySiteId(int siteId)//用simpledata可以生成带top 1的sql
        {
            var list = GetAdapter().Query<ApiContainerUploadRecord>("SiteId=@siteId", "AddTime desc",
                new Dictionary<string, object> { { "@siteId", siteId } }, null);
            if (list == null) return new List<ApiContainerUploadRecord>();
            return list;
        }

        public ApiContainerUploadRecord GetUploadRecordById(int id)
        {
            var record = GetAdapter().Query<ApiContainerUploadRecord>(id);
            return record;
        }

        public ApiContainerUploadRecord InsertUploadRecord(ApiContainerUploadRecord record)
        {
            var r = record;
            var adpt = GetAdapter();
            var itemRes = adpt.Insert(ref r);
            if (!itemRes) return null;
            return r;
        }

        public List<ApiContainerRouteMapping> GetRouteMappingsBySiteId(int siteId)
        {
            const string @where = "SiteId=@siteId";
            var paramDic = new Dictionary<string, object> { { "@siteId", siteId } };
            var adpt = GetAdapter();
            var list = adpt.Query<ApiContainerRouteMapping>(where, paramDic);
            return list;
        }

        public void DeleteSiteRouteMapping(int siteId)
        {
            var adpt = GetAdapter();
            adpt.DeleteApiContainerRouteMapping(siteId);            
        }

        public ApiContainerRouteMapping InsertSiteRouteMapping(ApiContainerRouteMapping mapping)
        {
            var m = mapping;
            var adpt = GetAdapter();
            var itemRes = adpt.InsertToDB(ref m);
            if (!itemRes) return null;
            return m;
        }

        public List<ApiContainerRouteMappingHistory> GetSiteRouteMappingHistoryByRecordId(int siteId,
            int recordId)
        {
            var list = GetAdapter()
                .Query<ApiContainerRouteMappingHistory>("SiteId=@siteId and UploadRecordId=@recordId",
                    new Dictionary<string, object> { { "@siteId", siteId }, { "@recordId", recordId } });
            if (list == null) return new List<ApiContainerRouteMappingHistory>();
            return list;
        }

        public ApiContainerRouteMappingHistory InsertRouteMappingHistory(ApiContainerRouteMappingHistory history)
        {
            var h = history;
            var adpt = GetAdapter();
            var itemRes = adpt.InsertToDB(ref h);
            if (!itemRes) return null;
            return h;
        }

        #endregion
    }



    #region RemoteLoader

    public class AssemblyDynamicLoader
    {
        private AppDomain _appDomain;
        private RemoteLoader _remoteLoader;
        public AssemblyDynamicLoader()
        {
            var domainName = "RefDomain" + Thread.CurrentThread.ManagedThreadId;
            var tmpLib = "TmpLib";
            var setup = new AppDomainSetup
            {
                ApplicationName = domainName + "ApplicationLoader",
                ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tmpLib),
                PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tmpLib, "private")
            };
            setup.CachePath = setup.ApplicationBase;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.ApplicationBase;

            _appDomain = AppDomain.CreateDomain(domainName, null, setup);
            try
            {
                var name = Assembly.GetExecutingAssembly().GetName().CodeBase;
                _remoteLoader =
                    (RemoteLoader)_appDomain.CreateInstanceFromAndUnwrap(name, typeof(RemoteLoader).FullName);
            }
            catch (Exception)
            {
                Unload();
                throw;
            }
        }

        public List<MethodInfoModel> GetMethods(string assemblyFile, string classFullName, BindingFlags flags)
        {
            List<MethodInfoModel> res = null;
            try
            {
                res = _remoteLoader.GetMethods(assemblyFile, classFullName, flags);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Unload();
            }
            return res;
        }

        public string[] GetExportedTypeNames(string assemblyFile)
        {
            string[] names = { };
            try
            {
                names = _remoteLoader.GetExportedTypeNames(assemblyFile);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Unload();
            }
            return names;
        }

        private void Unload()
        {
            try
            {
                if (_appDomain == null) return;
                AppDomain.Unload(_appDomain);
                _appDomain = null;
            }
            catch (CannotUnloadAppDomainException ex)
            {

            }
        }
    }

    public class RemoteLoader : MarshalByRefObject
    {
        private Assembly _assembly;

        public override object InitializeLifetimeService()
        {
            var aLease = (System.Runtime.Remoting.Lifetime.ILease)base.InitializeLifetimeService();
            if (aLease == null || aLease.CurrentState != System.Runtime.Remoting.Lifetime.LeaseState.Initial)
                return aLease;
            // 不过期
            aLease.InitialLeaseTime = TimeSpan.FromHours(1000);
            aLease.RenewOnCallTime = TimeSpan.FromHours(1000);
            aLease.SponsorshipTimeout = TimeSpan.FromHours(1000);
            return aLease;
        }

        public List<MethodInfoModel> GetMethods(string assemblyFile, string classFullName, BindingFlags flags)
        {
            var assembly = GetAssembly(assemblyFile);
            var cls = assembly.GetType(classFullName);
            var methods = cls.GetMethods(flags);
            var res = methods.Select(m =>
            {
                var o = new MethodInfoModel
                {
                    Name = m.Name,
                    IsStatic = m.IsStatic,
                    ReturnTypeName = m.ReturnType.Name,
                    ReturnTypeFullName = m.ReturnType.FullName,
                    ParmTypes =
                        m.GetParameters()
                            .Select(
                                p => new Tuple<string, string, string>(p.Name, p.ParameterType.Name, p.ParameterType.FullName))
                            .ToList()
                };
                return o;
            }).ToList();
            return res;
        }

        public string[] GetExportedTypeNames(string assemblyFile)
        {
            var assembly = GetAssembly(assemblyFile);
            var types = assembly.GetExportedTypes();
            var names = types.Select(t => t.FullName).ToArray();
            return names;
        }

        private Assembly GetAssembly(string assemblyFile)
        {
            return _assembly ?? (_assembly = Assembly.LoadFile(assemblyFile));
        }
    }

    /// <summary>
    /// MethodInfoModel
    /// </summary>
    /// <remarks>该类放到CreateInstanceFromAndUnwrap要求的程序集中</remarks>
    [Serializable]
    public class MethodInfoModel
    {
        public string Name { get; set; }

        public bool IsStatic { get; set; }

        public string ReturnTypeName { get; set; }

        public string ReturnTypeFullName { get; set; }

        public List<Tuple<string, string, string>> ParmTypes { get; set; }
    }
    #endregion
   
}
