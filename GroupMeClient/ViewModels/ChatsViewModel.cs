using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels
{
    public class ChatsViewModel : ViewModelBase
    {
        public ChatsViewModel()
        {
            ShowPopUp = new RelayCommand(() => ShowPopUpExecute(), () => true);
            IncrementValue = new RelayCommand(() => IncrementValueExecute(), () => true);
            ExampleValue = 0;
        }

        public ICommand ShowPopUp { get; private set; }

        public ICommand IncrementValue { get; private set; }

        private static void ShowPopUpExecute()
        {
            MessageBox.Show("Hello World!");
        }

        private void IncrementValueExecute()
        {
            ExampleValue += 1;
        }

        int _exampleValue;

        public int ExampleValue
        {
            get
            {
                return _exampleValue;
            }
            set
            {
                if (_exampleValue == value)
                    return;
                _exampleValue = value;
                RaisePropertyChanged("ExampleValue");
            }
        }
    }
}
