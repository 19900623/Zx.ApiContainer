using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Data;
using Zx.ApiAdmin.Entity.Admin;
using Zx.ApiAdmin.Services.DataAdapters;
using System.Collections;

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
            ApiContainerUploadRecord insertedRecord = Db.ApiContainerUploadRecord.Insert(record);
            return insertedRecord;
        }

        public List<ApiContainerUploadRecord> InsertUploadRecords(IEnumerable<ApiContainerUploadRecord> records)
        {
            List<ApiContainerUploadRecord> inserted =
                Db.ApiContainerUploadRecord.Insert(records.ToArray()).ToList<ApiContainerUploadRecord>();
            return inserted;
        }

        public List<ApiContainerRouteMapping> GetRouteMappingsBySiteId(int siteId)
        {
            List<ApiContainerRouteMapping> res = Db.ApiContainerRouteMapping.FindAllBySiteId(siteId);
            return res;
        }

        public void DeleteSiteRouteMapping(int siteId)
        {
            Db.ApiContainerRouteMapping.DeleteAll(Db.ApiContainerRouteMapping.SiteId == siteId);
        }

        public ApiContainerRouteMapping InsertSiteRouteMapping(ApiContainerRouteMapping mapping)
        {
            ApiContainerRouteMapping inserted = Db.ApiContainerRouteMapping.Insert(mapping);
            return inserted;
        }

        public List<ApiContainerRouteMapping> InsertSiteRouteMappings(IEnumerable<ApiContainerRouteMapping> mappings)
        {
            List<ApiContainerRouteMapping> inserted =
                Db.ApiContainerRouteMapping.Insert(mappings.ToArray()).ToList<ApiContainerRouteMapping>();
            return inserted;
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
            ApiContainerRouteMappingHistory inserted = Db.ApiContainerRouteMappingHistory.Insert(history);
            return inserted;
        }

        public List<ApiContainerRouteMappingHistory> InsertRouteMappingHistories(IEnumerable<ApiContainerRouteMappingHistory> histories)
        {
            List<ApiContainerRouteMappingHistory> inserted =
                Db.ApiContainerRouteMappingHistory.Insert(histories.ToArray()).ToList<ApiContainerRouteMappingHistory>();
            return inserted;
        }
    }
}
