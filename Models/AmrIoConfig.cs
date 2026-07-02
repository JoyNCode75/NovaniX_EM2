using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NovaniX_EM2.Models
{
    // 최상위 설정 클래스: 구동 모드별 설정 리스트를 가집니다.
    public class AmrIoConfig
    {
        public List<AmrIoModeConfig> Modes { get; set; } = new List<AmrIoModeConfig>();
    }

    // 구동 모드(차동/전방향 등)별 개별 I/O 설정 모음
    public class AmrIoModeConfig
    {
        public int ModeIndex { get; set; }
        public string ModeName { get; set; } = "";
        public int TotalInputCount { get; set; } = 10;
        public int TotalOutputCount { get; set; } = 10;
        public List<AmrIoNode> Inputs { get; set; } = new List<AmrIoNode>();
        public List<AmrIoNode> Outputs { get; set; } = new List<AmrIoNode>();
    }

    // 개별 I/O (0번~9번) 각각의 데이터 역할
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