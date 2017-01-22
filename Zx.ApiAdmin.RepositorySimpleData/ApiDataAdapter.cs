using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Data;
using Zx.ApiAdmin.Entity.Admin;
using Zx.ApiAdmin.Services.DataAdapters;

namespace Zx.ApiAdmin.RepositorySimpleData
{
    public class ApiDataAdapter : IApiContainerAdpt
    {
        /// <summary>
        /// db连接
        /// </summary>
        /// <remarks>
        /// 重构为从[ConfigDB].[dbo].[DBConnections]中读
        /// </remarks>
        private readonly string ApiContainerConn =
            @"Data Source=localhost;Initial Catalog=ApiContainer;User=sa;Password=asd123!!!;";

        private dynamic Db => Database.OpenConnection(ApiContainerConn);

        public List<ApiContainerSiteConfig> GetAllSite()
        {
            List<ApiContainerSiteConfig> res = Db.ApiContainerSiteConfig.All();
            return res;
        }

        public ApiContainerSiteConfig GetSiteById(int id)
        {
            ApiContainerSiteConfig res = Db.ApiContainerSiteConfig.Get(id);
            return res;
        }

        public List<ApiContainerUploadRecord> GetUploadRecordsBySiteId(int siteId)
        {
            List<ApiContainerUploadRecord> res =
                Db.ApiContainerUploadRecord.FindAllBySiteId(siteId).OrderByAddTimeDescending();
            return res;
        }

        public ApiContainerUploadRecord GetUploadRecordById(int id)
        {
            ApiContainerUploadRecord res = Db.ApiContainerUploadRecord.Get(id);
            return res;
        }

        public ApiContainerUploadRecord InsertUploadRecord(ApiContainerUploadRecord record)
        {
            throw new NotImplementedException();
        }

        public List<ApiContainerRouteMapping> GetRouteMappingsBySiteId(int siteId)
        {
            List<ApiContainerRouteMapping> res = Db.ApiContainerRouteMapping.FindAllBySiteId(siteId);
            return res;
        }

        public void DeleteSiteRouteMapping(int siteId)
        {
            throw new NotImplementedException();
        }

        public ApiContainerRouteMapping InsertSiteRouteMapping(ApiContainerRouteMapping mapping)
        {
            throw new NotImplementedException();
        }

        public List<ApiContainerRouteMappingHistory> GetSiteRouteMappingHistoryByRecordId(int siteId, int recordId)
        {
            List<ApiContainerRouteMappingHistory> res =
                Db.ApiContainerRouteMappingHistory.All()
                    .Where(Db.ApiContainerRouteMappingHistory.SiteId == siteId &&
                           Db.ApiContainerRouteMappingHistory.UploadRecordId == recordId);
            return res;
        }

        public ApiContainerRouteMappingHistory InsertRouteMappingHistory(ApiContainerRouteMappingHistory history)
        {
            throw new NotImplementedException();
        }
    }
}
