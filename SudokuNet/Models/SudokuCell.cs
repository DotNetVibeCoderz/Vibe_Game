using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SudokuNet.Models
{
    public class SudokuCell : INotifyPropertyChanged
    {
        private int _value;
        private bool _isFixed;
        private bool _isValid = true;
        private bool _isSelected;

        public int Row { get; set; }
        public int Col { get; set; }

        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public string Text => _value == 0 ? "" : _value.ToString();

        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                if (_isFixed != value)
                {
                    _isFixed = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsError));
                }
            }
        }

        public bool IsError => !_isValid;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}