using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zx.ApiAdmin.Entity.Admin;
using Zx.ApiAdmin.RepositorySimpleData;

namespace ConsoleTest
{
    public static class ReposSimpleDataTest
    {
        public static void Test1()
        {
            var adpt = new ApiDataAdapter();
            var resSites = adpt.GetAllSite();
            var resSite = adpt.GetSiteById(1);
            var resSiteRecords = adpt.GetUploadRecordsBySiteId(1);
            var resMappings = adpt.GetRouteMappingsBySiteId(1);
            var resHistory = adpt.GetSiteRouteMappingHistoryByRecordId(1, 1002);
        }

        public static void Test2()
        {
            var adpt = new ApiDataAdapter();
            ApiContainerUploadRecord insertedRes = adpt.InsertUploadRecord(new ApiContainerUploadRecord
            {
                AddTime = DateTime.Now,
                BackupFolder = "site0_xxx",
                Remark = "test",
                SiteId = 0
            });
        }

        public static void Test3()
        {
            ApiContainerUploadRecord[] array = {
                new ApiContainerUploadRecord
                {
                    AddTime = DateTime.Now,
                    BackupFolder = "site0_xxx",
                    Remark = "test",
                    SiteId = 0
                },
                new ApiContainerUploadRecord
                {
                    AddTime = DateTime.Now,
                    BackupFolder = "site0_xxx",
                    Remark = "test",
                    SiteId = 0
                },
                new ApiContainerUploadRecord
                {
                    AddTime = DateTime.Now,
                    BackupFolder = "site0_xxx",
                    Remark = "test",
                    SiteId = 0
                }
            };
            var adpt = new ApiDataAdapter();
            var res = adpt.InsertUploadRecords(array);
        }

        public static void Test4()
        {
            var adpt = new ApiDataAdapter();
            adpt.DeleteSiteRouteMapping(0);
        }
    }
}
