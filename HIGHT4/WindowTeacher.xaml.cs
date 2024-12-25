using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HIGHT4
{
    /// <summary>
    /// Логика взаимодействия для WindowTeacher.xaml
    /// </summary>
    public partial class WindowTeacher : Window
    {
        public WindowTeacher()
        {
            InitializeComponent();
            InitializeFolders();
        }

        private void InitializeFolders()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string baseFolder = System.IO.Path.Combine(desktopPath, "Тест сұрақтары QSTEM");

            // Папки для учителей
            string nuraiFolder = System.IO.Path.Combine(baseFolder, "Нұрай апай");
            string aybekFolder = System.IO.Path.Combine(baseFolder, "Айбек ағай");

            if (!Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder);
            if (!Directory.Exists(nuraiFolder)) Directory.CreateDirectory(nuraiFolder);
            if (!Directory.Exists(aybekFolder)) Directory.CreateDirectory(aybekFolder);
        }

        private void NuraiButton_Click(object sender, RoutedEventArgs e)
        {
            var testWindow = new MainWindow("Нұрай апай");
            testWindow.Show();
            this.Close();
        }

        private void AybekButton_Click(object sender, RoutedEventArgs e)
        {
            var testWindow = new MainWindow("Айбек ағай");
            testWindow.Show();
            this.Close();
        }

    }
}
