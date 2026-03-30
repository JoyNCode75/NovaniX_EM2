using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaniX_EM2.Models
{
    public class IoItemInfo
    {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsUse { get; set; } = false;
    }

    public class IoListCategory
    {
        public int Total { get; set; } = 8;
        public List<IoItemInfo> Items { get; set; } = new List<IoItemInfo>();
    }

    public class IoParameter
    {
        // ▼ 새로 추가된 통신 설정 항목 ▼
        public string IpAddress { get; set; } = "192.168.250.2";
        public int Port { get; set; } = 502;
        public IoListCategory Input { get; set; } = new IoListCategory();
        public IoListCategory Output { get; set; } = new IoListCategory();
    }
}