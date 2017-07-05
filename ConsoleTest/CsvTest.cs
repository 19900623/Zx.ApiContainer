using Simple.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class CsvTest
    {   
        private readonly string ApiContainerConn =
            @"server=10.168.100.30;uid=sa;pwd=*;database=UsedCarLog;Persist Security Info=True;Pooling=true;Max Pool Size=600";

        private readonly string Pathname = @"D:\";

        //private readonly string Filename = @"grmx1.csv";

        private readonly string Filename = @"个人明细.csv";


        private dynamic Db => Database.OpenConnection(ApiContainerConn);

        private List<SalesLeadsErrLog> _csvData = new List<SalesLeadsErrLog>();

        public void Test()
        {
            //List<TopicEntity> list = Db.Topic.All();
            CreateDataFromCsv();
            InsertDb();
        }

        private void CreateDataFromCsv()
        {
            var a = ReadDataFromCSV(Path.Combine(Pathname, Filename));
            //var dtRes = GetCsvData(Pathname, Filename);
            foreach (var r in a)
            {
                var lineStr = r[0].ToString();
                var cols = lineStr.Split(',');
                var liCols = cols.ToList();
                liCols.RemoveRange(0,5);
                var info = string.Join(",", liCols);

                int eType;
                int.TryParse(cols[2].ToString(), out eType);
                var model = new SalesLeadsErrLog
                {
                    dt = cols[0],
                    appkey = cols[1],
                    errortype = int.TryParse(cols[2], out eType) ? eType : 0,
                    errortext = cols[3],
                    errorcode = cols[4],
                    errorinfo = info
                };
                _csvData.Add(model);
            }           
        }

        private IList<Object[]> ReadDataFromCSV(string filePathName)
        {
            List<Object[]> ls = new List<Object[]>();
            StreamReader fileReader = new StreamReader(filePathName, Encoding.UTF8);
            string line = "";
            while (line != null)
            {
                line = fileReader.ReadLine();
                if (String.IsNullOrEmpty(line))
                    continue;
                String[] array = line.Split(';');
                ls.Add(array);
            }
            fileReader.Close();
            return ls;
        }

        private void InsertDb()
        {
            //List<SalesLeadsErrLog> inserted =
            //    Db.SalesLeadsErrLog.Insert(_csvData).ToList<SalesLeadsErrLog>();
            foreach (var salesLeadsErrLog in _csvData)
            {
                Db.SalesLeadsErrLog.Insert(salesLeadsErrLog);
            }
        }

    }

    public class SalesLeadsErrLog
    {
        //public int id { get; set; }
        public string dt { get; set; }
        public string appkey { get; set; }
        public int errortype { get; set; }
        public string errortext { get; set; }
        public string errorcode { get; set; }
        public string errorinfo { get; set; }
    }

    public class TopicEntity
    {
        public int TopicId { get; set; }

        public int Pid { get; set; }

        public int Cid { get; set; }
    }
}
