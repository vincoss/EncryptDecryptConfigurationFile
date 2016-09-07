using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace EncryptDecryptConfigurationFile.Services
{
    public interface IEncryptionService
    {
        void EncryptFile(string fileName, params string[] sections);
        void DecryptFile(string fileName, params string[] sections);
        string OpenFile();
    }

    public class EncryptionService : IEncryptionService
    {
        public static void EncryptConnectionString(bool encrypt, string fileName, params string[] sections)
        {
            if(sections.Length <= 0)
            {
                return;
            }

            try
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(fileName);

                foreach (var sectionName in sections)
                {
                    var configSection = configuration.GetSection(sectionName);
                    if ((!(configSection.ElementInformation.IsLocked)) && (!(configSection.SectionInformation.IsLocked)))
                    {
                        if (encrypt && !configSection.SectionInformation.IsProtected)
                        {
                            // Encrypt
                            configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        }

                        if (!encrypt && configSection.SectionInformation.IsProtected)
                        {
                            // Decrypt
                            configSection.SectionInformation.UnprotectSection();
                        }
                      
                        configSection.SectionInformation.ForceSave = true;
                    }
                }

                configuration.Save();
                Process.Start("notepad.exe", configuration.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        public void EncryptFile(string fileName, params string[] sections)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            if (File.Exists(fileName))
            {
                EncryptConnectionString(true, fileName, sections);
                return;
            }

            ShowError(fileName);
        }

        public void DecryptFile(string fileName, params string[] sections)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            if (File.Exists(fileName))
            {
                EncryptConnectionString(false, fileName, sections);
                return;
            }

            ShowError(fileName);
        }

        public string OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = ".Net Executables|*.exe";
            dialog.FileName = "";
            dialog.ShowDialog();
            return dialog.FileName;
        }

        private void ShowError(string fileName)
        {
            MessageBox.Show(fileName, "File does not exists");
        }
    }
}
