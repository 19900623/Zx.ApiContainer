using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
