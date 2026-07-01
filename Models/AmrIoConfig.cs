using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NovaniX_EM2.Models
{
    // 첫 번째 클래스: JSON 전체 설정을 담는 껍데기 역할
    public class AmrIoConfig
    {
        public int TotalInputCount { get; set; } = 10;
        public int TotalOutputCount { get; set; } = 10;

        public List<AmrIoNode> Inputs { get; set; } = new List<AmrIoNode>();
        public List<AmrIoNode> Outputs { get; set; } = new List<AmrIoNode>();
    }

    // 두 번째 클래스: 개별 I/O (0번~9번) 각각의 데이터 역할
    public class AmrIoNode : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string Name { get; set; } = "";
        public bool IsUsed { get; set; } = true;
        public string Address { get; set; } = "";

        private bool _isOn;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsOn
        {
            get => _isOn;
            set { _isOn = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}