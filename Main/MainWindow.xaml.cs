using LibGit2Sharp;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string path;

        private void Button_Click_GetPath(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog(this).GetValueOrDefault())
                Path_txt.Text = dialog.SelectedPath;

            path = dialog.SelectedPath;
        }

        private void Btn_Setup_ESX(object sender, RoutedEventArgs e)
        {
            SetupESX();
        }

        private void Btn_Setup_vRP(object sender, RoutedEventArgs e)
        {
            SetupVRP();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
        }

        //----------------------------------------------------------------------------------------------------------------------\\

        private void SetupFXServer()
        {
            //artifact-server
            Repository.Clone("https://github.com/Markholm0309/Cfx-Artifact-Server.git", path);

            //server-data
            Repository.Clone("https://github.com/citizenfx/cfx-server-data.git", $"{path}/resourceRepo");

            CreateCfgFile();
        }

        private void SetupESX()
        {
            SetupFXServer();

            Directory.CreateDirectory($@"{path}\resources\[ESX]");

            //resources
            Repository.Clone("https://github.com/esx-framework/esx_skin.git", $"{path}/resources/[ESX]/1"); 
            Repository.Clone("https://github.com/esx-framework/esx_jobs.git", $"{path}/resources/[ESX]/2");

            DirectoryCopy($@"{path}\resourceRepo", path, true);
        }

        private void SetupVRP()
        {
            Directory.CreateDirectory($@"{path}\resources\[vRP]");

            SetupFXServer();

            //resources
            Repository.Clone("https://github.com/esx-framework/esx_skin.git", $"{path}/resources/[vRP]/1");
            Repository.Clone("https://github.com/esx-framework/esx_jobs.git", $"{path}/resources/[vRP]/2");

            DirectoryCopy($@"{path}\resourceRepo", path, true);
        }

        private void CreateCfgFile()
        {
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent;
                       
            File.Copy($"{dir}/server.cfg", $"{path}/server.cfg");
        }

        private static void DeleteGitFiles(string sourceDir)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(sourceDir);

                foreach (string filePath in filePaths)
                {
                    if (filePath.Contains(".gitignore") || filePath.Contains(".md"))
                        File.Delete(filePath);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void DeleteGitDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (string subdirectory in Directory.EnumerateDirectories(directory))
                {
                    DeleteGitDirectory(subdirectory);
                }

                foreach (string fileName in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(fileName)
                    {
                        Attributes = FileAttributes.Normal
                    };

                    fileInfo.Delete();
                }

                Directory.Delete(directory);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Deletes .gitignore & readme
            DeleteGitFiles($@"{sourceDirName}");

            // Deletes .git folder
            DeleteGitDirectory($@"{sourceDirName}\.git");

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }

            // Deletes repo folder after copy to server folder
            var directory = new DirectoryInfo(sourceDirName) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }
    }
}
