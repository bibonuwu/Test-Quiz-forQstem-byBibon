using System;
using System.IO;
using System.Windows;

namespace HIGHT4
{
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

            // Список предметов
            var subjects = new[]
            {
                "Math", "English", "Informatiks", "Kazakh history", "Kazakh language",
                "World history", "Physics Lab", "Biology", "Algebra", "Geometry",
                "Geography", "Chemistry", "Psyholohy"
            };

            // Создание папок для каждого предмета
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            foreach (var subject in subjects)
            {
                string subjectFolder = System.IO.Path.Combine(baseFolder, subject);
                if (!Directory.Exists(subjectFolder))
                    Directory.CreateDirectory(subjectFolder);
            }
        }

        private void OpenMainWindow(string subjectName)
        {
            string teacherName = "Имя учителя"; // Убедитесь, что значение корректное
            var testWindow = new MainWindow(teacherName, subjectName); // Передаем выбранный предмет
            testWindow.Show();
            this.Close();
        }


        private void SubjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string subjectName)
            {
                OpenMainWindow(subjectName); // Передаем название предмета
            }
        }


    }
}
