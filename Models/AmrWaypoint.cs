using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NovaniX_EM2.Models
{
    public class AmrWaypoint : INotifyPropertyChanged
    {
        // Name 속성에 기본값을 주어 CS8618(초기화되지 않은 속성) 워닝도 방지합니다.
        public string Name { get; set; } = string.Empty;

        public double TargetX { get; set; }
        public double TargetY { get; set; }

        public double UiX { get; set; }
        public double UiY { get; set; }

        private string _status = "예정";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // [수정됨] CS8612 워닝 해결을 위해 '?' 추가
        public event PropertyChangedEventHandler? PropertyChanged;

        // [수정됨] 매개변수가 null을 허용하므로 string 타입 뒤에 '?' 추가
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}