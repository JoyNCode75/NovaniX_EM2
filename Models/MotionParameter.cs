using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaniX_EM2.Models
{
    /// <summary>
    /// 단일 모터 축의 파라미터 정보를 담는 데이터 클래스 (JSON 저장용)
    /// </summary>
// ▼ 새로 추가된 래퍼 클래스 (통신 설정 + 축 리스트)
    public class MotionSystemParameter
    {
        public string ComPort { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 115200;
        public Parity Parity { get; set; } = Parity.Even;
        public StopBits StopBits { get; set; } = StopBits.One;
        public List<MotionParameter> Axes { get; set; } = new List<MotionParameter>();
    }

    public class MotionParameter
    {
        public byte MotorNumber { get; set; }
        public string MotorName { get; set; } = string.Empty;
        public int MoveSpeed { get; set; } = 1000;
        public int MoveAccelDecel { get; set; } = 1000;
        public int JogSpeed { get; set; } = 500;
        public int JogAccelDecel { get; set; } = 500;
        public int NegativeLimit { get; set; } = -10000;
        public int PositiveLimit { get; set; } = 10000;
    }
}