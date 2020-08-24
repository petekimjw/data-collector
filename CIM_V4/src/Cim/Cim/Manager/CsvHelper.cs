using CIM.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Manager
{
    public class CsvHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region ColumnCSV

        /// <summary>
        /// 문자열 1줄 row를 csv 파일로 저장.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fileNamePrefix"></param>
        /// <returns></returns>
        public static bool WriteColumnCsvFile(List<string> headers, List<string> rows, string fileNamePrefix = null)
        {
            try
            {
                var dateTimeFormatOfColumnCsv = ConfigurationManager.AppSettings["DateTimeFormatOfColumnCsv"];
                bool writeColumnCsvFile = bool.Parse(ConfigurationManager.AppSettings["WriteColumnCsvFile"]);
                var columnCsvFilePath = ConfigurationManager.AppSettings["ColumnCsvFilePath"];
                if (string.IsNullOrEmpty(columnCsvFilePath))
                    columnCsvFilePath = @"logs\CSV";
                if (!Directory.Exists(columnCsvFilePath))
                    Directory.CreateDirectory(columnCsvFilePath);

                var fileFullName
                    = $@"{columnCsvFilePath}\{DateTime.Now.ToString("yyyy-MM-dd")}\{fileNamePrefix}.{DateTime.Now.ToString(dateTimeFormatOfColumnCsv)}.csv";

                if (writeColumnCsvFile)
                {
                    return WriteColumnCsvFileInternal(headers, rows, fileFullName);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"WriteColumnCsvFile ex={ex}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// List<Message>을 Column 형태의 csv 파일로 저장.
        /// 
        /// </summary>
        /// <param name="addressDatas"></param>
        /// <param name="fileNamePrefix"></param>
        public static bool WriteColumnCsvFile(List<AddressData> addressDatas, string fileNamePrefix = null)
        {
            try
            {
                if (!(addressDatas?.Count > 0)) return false;

                var dateTimeFormatOfColumnCsv = ConfigurationManager.AppSettings["DateTimeFormatOfColumnCsv"];
                bool writeColumnCsvFile = bool.Parse(ConfigurationManager.AppSettings["WriteColumnCsvFile"]);
                var columnCsvFilePath = ConfigurationManager.AppSettings["ColumnCsvFilePath"];
                if (string.IsNullOrEmpty(columnCsvFilePath))
                    columnCsvFilePath = @"logs\CSV";
                if (!Directory.Exists(columnCsvFilePath))
                    Directory.CreateDirectory(columnCsvFilePath);

                var fileFullName
                    = $@"{columnCsvFilePath}\{DateTime.Now.ToString("yyyy-MM-dd")}\{fileNamePrefix}.{DateTime.Now.ToString(dateTimeFormatOfColumnCsv)}.csv";

                if (writeColumnCsvFile)
                {
                    var results = CsvHelper.ReshapeColumnDataTable(addressDatas as List<AddressData>);
                    return CsvHelper.WriteColumnCsvFileInternal(results, fileFullName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
            return true;
        }


        public static bool WriteColumnCsvFileInternal(List<string> headers, List<string> rows, string fileFullName, Encoding encoding = null)
        {
            try
            {
                if (encoding == null) encoding = Encoding.Default;

                var folder = fileFullName.Substring(0, fileFullName.LastIndexOf("\\"));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fs = new FileStream(fileFullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (var sw = new StreamWriter(fs, encoding))
                {
                    //header
                    byte[] array = new byte[10];
                    bool hasHeader = fs.Read(array, 0, 10) > 0;
                    fs.Position = fs.Length;

                    if (hasHeader == false)
                    {
                        sw.WriteLine(string.Join(",", headers));
                    }

                    //row
                    foreach (string item in rows)
                    {
                        sw.WriteLine(item);
                    }
                    sw.Close();
                }
                fs.Close();

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"WriteColumnCsvFileInternal ex={ex}");
                return false;
            }

        }

        /// <summary>
        /// DataTable을 ColumnCSV 파일로 저장
        /// </summary>
        /// <param name="results"></param>
        /// <param name="fileFullName"></param>
        public static bool WriteColumnCsvFileInternal(DataTable results, string fileFullName, Encoding encoding = null)
        {
            try
            {
                if (encoding == null) encoding = Encoding.Default;

                var folder = fileFullName.Substring(0, fileFullName.LastIndexOf("\\"));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                bool hasHeader = false;
                (hasHeader, fileFullName) =
                    MatchCsvHeaderAndFileName(fileFullName, results.Columns.Cast<DataColumn>().Select(m => m.ColumnName).ToList(), encoding);

                var fs = new FileStream(fileFullName, FileMode.Append, FileAccess.Write);
                using (var sw = new StreamWriter(fs, encoding))
                {
                    //header
                    if (hasHeader == false)
                    {
                        var header = "";
                        foreach (DataColumn column in results.Columns)
                        {
                            if (string.IsNullOrEmpty(header))
                                header = column.ColumnName;
                            else
                                header = header + "," + column.ColumnName;
                        }
                        sw.WriteLine(header);
                    }

                    //row
                    int columnCount = results.Columns.Count;
                    foreach (DataRow item in results.Rows)
                    {
                        var data = "";
                        for (int i = 0; i < columnCount; i++)
                        {
                            string value = item[i]?.ToString();

                            if (string.IsNullOrEmpty(data))
                                data = value;
                            else
                                data = data + "," + value;
                        }
                        sw.WriteLine(data);
                    }
                    sw.Close();
                }
                fs.Close();

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"WriteColumnCsvFile ex={ex}");
            }
            return false;
        }


        /// <summary>
        /// CSV 파일의 헤더와 columns와 비교하여 틀리면 새로운 파일이름 반환
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static (bool, string) MatchCsvHeaderAndFileName(string fileFullName, List<string> columns, Encoding encoding = null)
        {
            bool hasHeader = false;
            bool mustCreateNewFile = false;
            try
            {
                if (encoding == null) encoding = Encoding.Default;

                var fs = new FileStream(fileFullName, FileMode.OpenOrCreate, FileAccess.Read);
                //header
                //주의! 헤더의 순서가 틀리면 새로운 파일로 만든다.
                using (var sr = new StreamReader(fs, encoding))
                {
                    var line = sr.ReadLine();

                    var headers = line?.Split(',');
                    if (headers?.FirstOrDefault() == "Time")
                    {
                        hasHeader = true;

                        for (int i = 0; i < headers.Length; i++)
                        {
                            if (headers[i] != columns[i])
                            {
                                hasHeader = false;
                                mustCreateNewFile = true;
                                logger.Debug($"WriteColumnCsvFileInternal new fileName {headers[i]} != {columns[i]}");
                                break;
                            }
                        }
                    }
                    sr.Close();
                }

                if (mustCreateNewFile)
                {
                    //파일이 이미 있으면 fileName-{count}.csv 로 변경
                    int count = 0;
                    var fileName = fileFullName.Substring(fileFullName.LastIndexOf("\\") + 1);
                    int index = fileName.LastIndexOf('-');
                    if (index == -1)
                        fileFullName = $"{fileFullName.Replace(".csv", "-1.csv")}";
                    else
                    {
                        int.TryParse(fileFullName.Substring(index).Replace(".csv", ""), out count);
                        fileFullName = $"{fileFullName.Substring(0, index)}-{count}.csv";
                    }

                    return MatchCsvHeaderAndFileName(fileFullName, columns, encoding);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return (hasHeader, fileFullName);
        }

        #endregion

        /// <summary>
        /// List<Message>를 ColumnCSV 형태로 변환하기 위해 DataTable 형태로 변환.
        /// 메시지를 DateTime으로 그룹핑하므로, 무조건 같은시간대의 메시지여야 같은 row 에 배치된다.
        /// </summary>
        /// <param name="addressDatas"></param>
        /// <param name="sortColumns"></param>
        /// <returns></returns>
        public static DataTable ReshapeColumnDataTable(
            List<AddressData> addressDatas, Func<List<string>, List<string>> sortColumns = null)
        {
            var results = new DataTable();
            results.Columns.Add("Time");

            //datetime group
            var messageGroups = addressDatas.GroupBy(m => m.Time).ToList();

            //Columns
            var columns = (from raw in messageGroups.FirstOrDefault()
                           select raw.VariableName).ToList();
            //사용자 정의 Sort
            if (sortColumns != null)
                columns = sortColumns(columns);

            foreach (var column in columns)
            {
                if (!results.Columns.Contains(column))
                {
                    results.Columns.Add(column);
                }
            }

            //Add dataRow

            #region 주의! Parallel.ForEach 시 비정상 대용량 GB단위 로그파일 생성됨

            //Parallel.ForEach(messageGroups, messageGroup =>
            //{
            //    var dataRow = results.NewRow();
            //    dataRow["Time"] = messageGroup.Key.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //    foreach (var item in messageGroup)
            //    {
            //        if (results.Columns.Contains(item.Variable))
            //        {
            //            dataRow[item.Variable] = item.Value;
            //        }
            //    }
            //    results.Rows.Add(dataRow);
            //}); 
            #endregion

            #region foreach

            foreach (var messageGroup in messageGroups)
            {
                var dataRow = results.NewRow();
                dataRow["Time"] = messageGroup.Key.ToString("yyyy-MM-dd HH:mm:ss.fff");
                foreach (var item in messageGroup)
                {
                    if (results.Columns.Contains(item.VariableName))
                    {
                        dataRow[item.VariableName] = item.Value;
                    }
                }
                results.Rows.Add(dataRow);
            };
            #endregion

            return results;
        }

    }
}
