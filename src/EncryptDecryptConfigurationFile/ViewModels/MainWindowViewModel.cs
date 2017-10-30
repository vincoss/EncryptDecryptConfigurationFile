using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using EncryptDecryptConfigurationFile.Commands;
using EncryptDecryptConfigurationFile.Services;


namespace EncryptDecryptConfigurationFile.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IEncryptionService _service;

        public MainWindowViewModel() : this(new EncryptionService())
        {
            EncryptCommand = new DelegateCommand(OnEncryptCommand, OnCanEncryptCommand);
            DecryptCommand = new DelegateCommand(OnDecryptCommand, OnCanDecryptCommand);
            SelectCommand = new DelegateCommand(OnSelectedCommand);

            AppSettingsCheckBox = true;
            ConnectionStringsCheckBox = true;
            WebConfigCheckBox = false;

            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        public MainWindowViewModel(IEncryptionService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            _service = service;
        }
        
        #region Event handlers

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((Command)EncryptCommand).RaiseCanExecuteChanged();
            ((Command)DecryptCommand).RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private methods

        private bool GetCanExecute()
        {
            return (AppSettingsCheckBox || ConnectionStringsCheckBox) && string.IsNullOrEmpty(SelectedPath) == false;
        }

        private string[] GetSections()
        {
            var sections = new List<string>();

            if (AppSettingsCheckBox)
            {
                sections.Add("appSettings");
            }

            if (ConnectionStringsCheckBox)
            {
                sections.Add("connectionStrings");
            }

            return sections.ToArray();
        }

        #endregion

        #region Command methods

        private void OnEncryptCommand()
        {
            _service.EncryptFile(SelectedPath, GetSections());
        }

        private bool OnCanEncryptCommand()
        {
            return GetCanExecute();
        }

        private void OnDecryptCommand()
        {
            _service.DecryptFile(SelectedPath, GetSections());
        }

        private bool OnCanDecryptCommand()
        {
            return GetCanExecute();
        }

        private void OnSelectedCommand()
        {
            this.SelectedPath = _service.OpenFile(WebConfigCheckBox);
        }

        #endregion

        #region Commands

        public ICommand EncryptCommand { get; private set; }

        public ICommand DecryptCommand { get; private set; }

        public ICommand SelectCommand { get; set; }

        #endregion

        #region Properties

        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                if (_selectedPath != value)
                {
                    _selectedPath = value;
                    OnPropertyChanged("SelectedPath");
                }
            }
        }

        private bool _appSettingsCheckBox;

        public bool AppSettingsCheckBox
        {
            get { return _appSettingsCheckBox; }
            set
            {
                if (_appSettingsCheckBox != value)
                {
                    _appSettingsCheckBox = value;
                    OnPropertyChanged("AppSettingsCheckBox");
                }
            }
        }

        private bool _connectionStringsCheckBox;

        public bool ConnectionStringsCheckBox
        {
            get { return _connectionStringsCheckBox; }
            set
            {
                if (_connectionStringsCheckBox != value)
                {
                    _connectionStringsCheckBox = value;
                    OnPropertyChanged("ConnectionStringsCheckBox");
                }
            }
        }

        private bool _webConfigCheckBox;

        public bool WebConfigCheckBox
        {
            get { return _webConfigCheckBox; }
            set
            {
                if (_webConfigCheckBox != value)
                {
                    _webConfigCheckBox = value;
                    OnPropertyChanged("WebConfigCheckBox");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged; 

        #endregion
    }
}
