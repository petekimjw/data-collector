using Cim.Domain.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.Service
{
    public interface IDbSyncService
    {
        Task<bool> Sync(List<Controller> controllers);
    }

    /// <summary>
    /// 어드레스맵의 내용을 pie_v2 데이터베이스에 t_dvc_info, t_var_info 내용을 PIE Db Upsert 한다(Update+Insert)
    /// </summary>
    public class PieDbSyncService : IDbSyncService
    {
        #region 속성
        /// <summary>
        /// 연결정보
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// 프로바이더
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// CIM에서 사용할 수 있는 최대 시퀀스 값(해당 값 이상의 시퀀스는 PIE전용)
        /// </summary>
        private const int MaxSeq = 100000;
        private const string DvcInfoTable = "t_dvc_info";
        private const string VarInfoTable = "t_var_info";
        private const string DvcSeqColumn = "dvc_seq";
        private const string VarSeqColumn = "var_seq";
        private const string DvcIdColumn = "dvc_id";
        private const string VarIdColumn = "var_id";
        private const string DvcNmColumn = "dvc_nm";
        private const string VarNmColumn = "var_nm";

        /// <summary>
        /// 프로바이터 팩토리 클래스
        /// </summary>
        private DbProviderFactory dbProviderFactory = null;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region 초기화
        public PieDbSyncService()
        {
            try
            {
                var connectionStringSetting = ConfigurationManager.ConnectionStrings["pie"];
                if (connectionStringSetting == null)
                {
                    logger.Error("\"pie\" ConnectionString을 찾을 수 없습니다.");
                    return;
                }

                ProviderName = connectionStringSetting.ProviderName;
                ConnectionString = connectionStringSetting.ConnectionString;
                dbProviderFactory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (Exception ex)
            {
                logger?.Error(ex);
            }
        }

        public PieDbSyncService(string providerName, string connectionString)
        {
            try
            {
                ProviderName = providerName;
                ConnectionString = connectionString;
                dbProviderFactory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (Exception ex)
            {
                logger?.Error(ex);
            }
        }
        #endregion

        #region Connect
        public async Task<bool> Connect()
        {
            DbConnection connection = null;

            try
            {
                connection = CreateDbConnection();
                await connection.OpenAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
                return false;
            }
            finally
            {
                connection?.Close();
                connection?.Dispose();
            }
        }

        private DbConnection CreateDbConnection()
        {
            var dbConnection = dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = ConnectionString;

            return dbConnection;
        }
        #endregion

        #region Sync
        /// <summary>
        /// 엑셀에서 읽어온 controllers 를 PIE DB에 Insert 한다
        /// </summary>
        /// <param name="controllers"></param>
        /// <returns></returns>
        public async Task<bool> Sync(List<Controller> controllers)
        {
            if (!(controllers?.Count > 0))
                return true;

            bool result = false;

            try
            {
                var dvcInfoTable = await GetDvcInfoTable();
                var varInfoTable = await GetVarInfoTable();

                var dvcInfoSeq = GetMaxSequence(dvcInfoTable, DvcSeqColumn) + 1;
                var varInfoSeq = GetMaxSequence(varInfoTable, VarSeqColumn) + 1;

                var dvcInfoIds = dvcInfoTable.AsEnumerable().Select(m => m.Field<string>(DvcIdColumn))?.ToList();
                var varInfoIds = varInfoTable.AsEnumerable().Select(m => m.Field<string>(VarIdColumn))?.ToList();

                foreach (var controller in controllers)
                {
                    if (controller?.IsUsed != true)
                        continue;

                    var queries = new List<string>();

                    var deviceIds = controller?.AddressMaps.Select(m => m.DeviceId)?.Distinct()?.ToList();
                    foreach (var deviceId in deviceIds)
                    {
                        if (dvcInfoIds.Contains(deviceId, StringComparer.OrdinalIgnoreCase) == false)
                        {
                            queries.Add($"insert into {DvcInfoTable} ({DvcSeqColumn}, {DvcIdColumn}, {DvcNmColumn}) values({dvcInfoIds}, '{deviceId}', '{deviceId}');");
                            dvcInfoSeq++;
                        }
                    }

                    foreach (var address in controller.AddressMaps.Where(m => m.IsUsed == true))
                    {
                        if (varInfoIds.Contains(address.VariableId, StringComparer.OrdinalIgnoreCase) == false)
                        {
                            queries.Add($"insert into {VarInfoTable} ({VarSeqColumn}, {VarIdColumn}, {VarNmColumn}) values({varInfoSeq}, '{address.VariableId}', '{address.VariableName}');");
                            varInfoSeq++;
                        }
                    }

                    result = await InsertQuery(queries);
                    if (!result)
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return result;
        }

        public async Task<bool> InsertQuery(List<string> queries)
        {
            if (queries.Count == 0)
                return true;

            DbConnection connection = CreateDbConnection();
            DbTransaction transaction = null;
            int executedCount = 0;
            try
            {
                logger?.Info($"[InsertQuery] Start! count={queries.Count}");

                await connection.OpenAsync();
                transaction = connection.BeginTransaction();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    foreach (string query in queries)
                    {
                        logger?.Debug(query);

                        cmd.CommandText = query;
                        executedCount += await cmd.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();

                logger?.Info($"[InsertQuery] End! count={executedCount}");

                return true;
            }
            catch (Exception ex)
            {
                logger?.Error(ex);

                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return false;
            }
            finally
            {
                connection?.Close();
            }
        } 
        #endregion

        #region Get PIE Table
        private DataTable GetDataTable(string table)
        {
            DataTable dt = new DataTable();

            try
            {
                using (var connection = CreateDbConnection())
                using (var cmd = connection.CreateCommand())
                using (var adapter = dbProviderFactory.CreateDataAdapter())
                {
                    cmd.CommandText = $"select * from {table};";
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                logger?.Error(ex);
            }

            return dt;
        }

        private async Task<DataTable> GetDvcInfoTable()
        {
            DataTable dataTable = null;
            await Task.Run(new Action(() =>
            {
                dataTable = GetDataTable(DvcInfoTable);
            }));
            return dataTable;
        }

        private async Task<DataTable> GetVarInfoTable()
        {
            DataTable dataTable = null;
            await Task.Run(new Action(() =>
            {
                dataTable = GetDataTable(VarInfoTable);
            }));
            return dataTable;
        }

        private int GetMaxSequence(DataTable table, string column)
        {
            try
            {
                if (table.Columns.Contains(column) == false)
                {
                    throw new Exception($"{table} 테이블에 {column} 컬럼이 존재하지 않습니다.");
                }

                var result = table?.AsEnumerable().Select(m => m.Field<int>(column)).Where(m => m < MaxSeq)?.ToList();
                return result.Any() ? result.Max() : 0;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
                return -1;
            }
        } 
        #endregion

    }
}
