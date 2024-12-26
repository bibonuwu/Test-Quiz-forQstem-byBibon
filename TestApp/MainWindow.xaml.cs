using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace TestApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FirebaseBaseUrl = "https://test-qstem-default-rtdb.firebaseio.com/";
        private Dictionary<string, Dictionary<string, List<Question>>> _subjectsAndTests;

        public MainWindow()
        {
            InitializeComponent();
            LoadSubjectsAndTests();

        }

        private async void LoadSubjectsAndTests()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"{FirebaseBaseUrl}.json";
                    var response = await client.GetStringAsync(url);

                    // Десериализация данных в правильную структуру
                    _subjectsAndTests = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<Question>>>>(response);

                    if (_subjectsAndTests != null && _subjectsAndTests.Any())
                    {
                        SubjectsComboBox.ItemsSource = _subjectsAndTests.Keys;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SubjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubjectsComboBox.SelectedItem is string selectedSubject && _subjectsAndTests.ContainsKey(selectedSubject))
            {
                TestsComboBox.ItemsSource = _subjectsAndTests[selectedSubject].Keys;
            }
        }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsComboBox.SelectedItem is string selectedSubject &&
                TestsComboBox.SelectedItem is string selectedTest &&
                _subjectsAndTests.ContainsKey(selectedSubject) &&
                _subjectsAndTests[selectedSubject].ContainsKey(selectedTest))
            {
                var questions = _subjectsAndTests[selectedSubject][selectedTest];
                var testWindow = new TestWindow(selectedTest, questions);
                testWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите предмет и тест!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class Question
    {
        public string Text { get; set; }
        public int Points { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class Answer
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

}

