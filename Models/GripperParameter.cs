using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace NovaniX_EM2.Models
{
    public class GripperParameter
    {
        public string ComPort { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 115200;
        public Parity Parity { get; set; } = Parity.Even;
        public StopBits StopBits { get; set; } = StopBits.One;
        public byte SlaveId { get; set; } = 1; // 그리퍼 기본 국번
    }
}