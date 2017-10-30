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
        string OpenFile(bool isWebConfigFile);
    }

    public class EncryptionService : IEncryptionService
    {
        private static void EncryptFromExe(bool encrypt, string fileName, params string[] sections)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(fileName);
            EncryptSections(encrypt, configuration, sections);
        }

        private static void EncryptWebConfig(bool encrypt, string fileName, params string[] sections)
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = fileName
            };
            
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            
            EncryptSections(encrypt, configuration, sections);
        }

        private static void EncryptSections(bool encrypt, Configuration configuration, params string[] sections)
        {
            if(sections.Length <= 0)
            {
                return;
            }

            try
            {
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
                if(fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    EncryptFromExe(true, fileName, sections);
                }
                else
                {
                    EncryptWebConfig(true, fileName, sections);
                }
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
                if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    EncryptFromExe(false, fileName, sections);
                }
                else
                {
                    EncryptWebConfig(false, fileName, sections);
                }
                return;
            }

            ShowError(fileName);
        }

        public string OpenFile(bool isWebConfigFile)
        {
            string filter = ".Net Executables|*.exe";

            if(isWebConfigFile)
            {
                filter = "Web.config|Web.config";
            }

            var dialog = new OpenFileDialog();
            dialog.Filter = filter;
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
