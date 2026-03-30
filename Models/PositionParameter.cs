using System.Collections.Generic;

namespace NovaniX_EM2.Models
{
    public class PositionPoint
    {
        public int PositionNumber { get; set; } // ★ 포지션 번호 추가
        public bool IsUsed { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; } = 0;
    }

    public class PositionParameter
    {
        public byte MotorNumber { get; set; }
        public string MotorName { get; set; } = string.Empty;
        public List<PositionPoint> Positions { get; set; } = new List<PositionPoint>();
    }
}