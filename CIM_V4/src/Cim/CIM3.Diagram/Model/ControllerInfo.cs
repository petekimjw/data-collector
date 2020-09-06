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
    /// T_CONT_INFO
    /// </summary>
    [Table("T_CONT_INFO")]
    public class ControllerInfo
    {
        [Column("CONT_SEQ"), Key]
        public int Sequence { get; set; }

        [Column("LCSVR_SEQ"), Required]
        public int LocalServerInfoSequence { get; set; }

        [Column("USE_YN")]
        public bool UseYN { get; set; } = true;

        [Column("LOG_SAVE_YN")]
        public bool UseLogSave { get; set; } = true;

        [Column("RAW_DATA_SAVE_YN")]
        public bool UseRawDataSave { get; set; } = true;

        [Column("CONT_ID"), Required]
        public string ID { get; set; }

        [Column("CONT_NM"), Required]
        public string Name { get; set; }

        [Column("CONT_DTL")]
        public string Detail { get; set; }

        [Column("CONT_TYPE"), Required]
        public string Type { get; set; }

        [Column("CONT_IP"), Required]
        public string IP { get; set; }

        [Column("CONT_PORT"), Required]
        public int Port { get; set; }

        [Column("PRTCL_TYPE"), Required]
        public EProtocolTypeFlags Protocol { get; set; }

        /// <summary>
        /// 수집주기(Trace, Data)
        /// </summary>
        [Column("CLLCT_INTERVAL")]
        public int Interval { get; set; } = 1000 * 5; // 5초

        /// <summary>
        /// 수집주기(DeviceStatus)
        /// </summary>
        [Column("STATUS_INTERVAL")]
        public int StatusInterval { get; set; } = 1000 * 1; // 1초

        /// <summary>
        /// 2019.09.18 추가 (hskim)
        /// 수집주기(Alarm)
        /// </summary>
        [NotMapped]
        [Column("ALARM_INTERVAL")]
        public int AlarmInterval { get; set; } = 1000 * 3; // 3초

        /// <summary>
        /// 2019.09.18 추가 (hskim)
        /// 수집주기(Useless)
        /// </summary>
        [NotMapped]
        [Column("USELESS_INTERVAL")]
        public int UselessInterval { get; set; } = 1000 * 60 * 1; // 1분

        #region Macro Property
        [NotMapped]
        [Column("LCSVR_NM")]
        public string LCSVR_NM { get; set; }
        #endregion

        #region ETC
        /// <summary>
        /// 프로토콜 정보를 가져옵니다.
        /// </summary>
        //[NotMapped]
        //public ProtocolType Protocol => ProtocolTypeValue.TryParseEnum<ProtocolType>(ProtocolType.None);

        /// <summary>
        /// 장비 목록을 가져오거나 설정합니다.
        /// </summary>
        [NotMapped]
        public List<DeviceInfo> DeviceInfoList { get; set; }

        /// <summary>
        /// 어드레스 템플릿 정보를 가져오거나 설정합니다.
        /// </summary>
        [NotMapped]
        public List<AddressMapTemplateInfo> AddressMapTemplateInfoes { get; set; }

        /// <summary>
        /// 어드레스 맵 정보를 가져오거나 설정합니다.
        /// </summary>
        [NotMapped]
        public List<AddressMapInfo> AddressMapInfoes { get; set; }

        /// <summary>
        /// 알람코드 목록을 가져오거나 설정합니다.
        /// </summary>
        [NotMapped]
        public List<AlarmCodeInfo> AlarmCodeInfoList { get; set; }

        /// <summary>
        /// MELSEC 통신시 필요한 설정 정보를 가져옵니다.
        /// </summary>
        [NotMapped]
        public MelsecInfo MelsecInfo { get; set; }
        private async Task<ICollection<MelsecInfo>> getListAsync()
        {
            return null;
        }

        /// <summary>
        /// 시리얼 통신 시 필요한 설정 정보를 가져옵니다.
        /// </summary>
        [NotMapped]
        public SerialInfo SerialInfo { get; set; }

        /// <summary>
        /// 공유파일 정보를 가져오거나 설정합니다.
        /// </summary>
        [NotMapped]
        public List<SharedFileInfo> SharedFileInfos { get; set; }
        #endregion
    }

    public enum EProtocolTypeFlags
    {

    }

    public class SharedFileInfo { }

    public class SerialInfo { }

    public class MelsecInfo { }

    public class AlarmCodeInfo { }



    public class AddressMapTemplateInfo { }

    public class DeviceInfo { }

    


}
