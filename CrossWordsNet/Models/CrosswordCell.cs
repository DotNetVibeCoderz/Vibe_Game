using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrossWordsNet.Models
{
    public class CrosswordCell : INotifyPropertyChanged
    {
        private bool _isHighlighted;
        private string _userInput = string.Empty;

        public int Row { get; set; }
        public int Col { get; set; }
        public char? Letter { get; set; }
        
        public string UserInput 
        { 
            get => _userInput; 
            set 
            {
                if (_userInput != value)
                {
                    _userInput = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCorrect));
                }
            }
        }
        
        public bool IsBlock { get; set; } = true;
        public bool IsCell => !IsBlock;

        public int? Number { get; set; } 

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCorrect => !string.IsNullOrEmpty(UserInput) && UserInput.Length > 0 && Letter.HasValue && UserInput[0].ToString().ToUpper() == Letter.Value.ToString().ToUpper();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}