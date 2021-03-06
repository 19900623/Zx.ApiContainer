﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zx.ApiAdmin.Entity.Admin;

namespace Zx.ApiAdmin.Services.DataAdapters
{
    public interface IApiContainerAdpt
    {
        List<ApiContainerSiteConfig> GetAllSite();

        ApiContainerSiteConfig GetSiteById(int id);

        List<ApiContainerUploadRecord> GetUploadRecordsBySiteId(int siteId);

        ApiContainerUploadRecord GetUploadRecordById(int id);

        ApiContainerUploadRecord InsertUploadRecord(ApiContainerUploadRecord record);

        List<ApiContainerUploadRecord> InsertUploadRecords(IEnumerable<ApiContainerUploadRecord> records);

        List<ApiContainerRouteMapping> GetRouteMappingsBySiteId(int siteId);

        void DeleteSiteRouteMapping(int siteId);

        ApiContainerRouteMapping InsertSiteRouteMapping(ApiContainerRouteMapping mapping);

        List<ApiContainerRouteMapping> InsertSiteRouteMappings(IEnumerable<ApiContainerRouteMapping> mappings);

        List<ApiContainerRouteMappingHistory> GetSiteRouteMappingHistoryByRecordId(int siteId, int recordId);

        ApiContainerRouteMappingHistory InsertRouteMappingHistory(ApiContainerRouteMappingHistory history);

        List<ApiContainerRouteMappingHistory> InsertRouteMappingHistories(
            IEnumerable<ApiContainerRouteMappingHistory> histories);
    }
}
