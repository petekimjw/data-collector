using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram.Model
{
    /// <summary>
    /// T_LCSVR_INFO
    /// </summary>
    [Table("T_LCSVR_INFO")]
    public class LocalServerInfo : ICloneable
    {
        [Column("LCSVR_SEQ"), Key]
        public int Sequence { get; set; }

        [Column("LCSVR_NM"), Required]
        public string Name { get; set; }

        #region ETC
        private List<ControllerInfo> mControllers;
        public async Task<IEnumerable<ControllerInfo>> GetControllersAsync()
        {
            return null;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
