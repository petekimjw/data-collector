using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram.Bind
{
    /// <summary>
    /// 설비와 I/F 하기 위한 프토토콜별 기능을 정의
    /// </summary>
    public interface IBindBehavior : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// 컨트롤러/센서 상태를 가져오거나 설정
        /// </summary>
        EControllerStatusKind ControllerStatus { get; set; }

        /// <summary>
        /// 수집할 어드레스 맵 목록을 반환
        /// </summary>
        List<AddressMapInfo> GetAddressMapList();

        /// <summary>
        /// 수집한 데이터의 목록을 반환
        /// </summary>
        List<T> Read<T>(List<AddressMapInfo> addressMaps) where T : Message, new();

        /// <summary>
        /// 원하는 어드레스 에 데이터를 작성
        /// </summary>
        bool Write(AddressMapInfo addressMap, object value);

        /// <summary>
        /// 통신을 종료
        /// </summary>
        Task<bool> CloseAsync();
    }

    public enum EControllerStatusKind { }

    public class BaseDeviceBehavior
    {
        public ControllerInfo Controller { get; }
        
        public ICancel Cancel { get; }
        public bool IsCancelRequested => (Cancel != null && Cancel.IsCancelRequested);


    }
}
