using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.Service
{
    public interface IDbSyncService
    {
        bool Sync(string connectionString);
    }

    /// <summary>
    /// 어드레스맵의 내용을 pie_v2 데이터베이스에 t_dvc_info, t_var_info 내용을 Upsert 한다(Update+Insert)
    /// </summary>
    public class DbSyncService : IDbSyncService
    {
        public bool Sync(string connectionString)
        {
            return true;
        }
    }
}
