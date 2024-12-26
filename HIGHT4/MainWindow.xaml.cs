using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;

namespace HIGHT4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _teacherName;
        private string _saveFolder;
        private bool _isTestModified = false; // Флаг изменения теста
        private const string FirebaseBaseUrl = "https://test-qstem-default-rtdb.firebaseio.com/";
        private bool _isManualInput = false; // Флаг для определения ручного ввода
        private string _currentSubject = "Name subject";


        public MainWindow(string teacherName, string selectedSubject)
        {
            InitializeComponent();

            // Установка переданных значений
            _teacherName = teacherName ?? throw new ArgumentNullException(nameof(teacherName));
            _currentSubject = selectedSubject ?? throw new ArgumentNullException(nameof(selectedSubject));

            // Устанавливаем метку для текущего предмета
            SubjectLabel.Content = _currentSubject;

            // Формирование пути для сохранения
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string baseFolder = System.IO.Path.Combine(desktopPath, "Тест сұрақтары QSTEM");
            _saveFolder = System.IO.Path.Combine(baseFolder, _teacherName);

            InitializeFolders(); // Создаем папки
            LoadSavedTests(); // Загружаем сохраненные тесты
            _ = LoadSavedTestsFromFirebase();
        }


        private void InitializeFolders()
        {
            if (!Directory.Exists(_saveFolder))
            {
                Directory.CreateDirectory(_saveFolder);
            }
        }
        private void LoadSavedTests()
        {
            if (Directory.Exists(_saveFolder))
            {
                // Формируем путь для текущего предмета
                string subjectFolder = System.IO.Path.Combine(_saveFolder, _currentSubject);

                if (Directory.Exists(subjectFolder))
                {
                    // Загружаем только файлы текущего предмета
                    var files = Directory.GetFiles(subjectFolder, "*.json");
                    if (files.Any())
                    {
                        SavedTestsListBox.ItemsSource = files.Select(System.IO.Path.GetFileNameWithoutExtension).ToList();
                    }
                    else
                    {
                        SavedTestsListBox.ItemsSource = null; // Очищаем список, если нет тестов
                        MessageBox.Show($"Для предмета '{_currentSubject}' нет сохраненных тестов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    SavedTestsListBox.ItemsSource = null;
                    MessageBox.Show($"Для предмета '{_currentSubject}' папка отсутствует.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }






        private async void SaveCurrentTest()
        {
            SaveCurrentQuestion(); // Сохраняем текущий вопрос

            if (_questions == null || !_questions.Any())
            {
                MessageBox.Show("Нет вопросов для сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TestNameTextBox.Text))
            {
                MessageBox.Show("Введите имя теста перед сохранением.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаем папку для текущего предмета, если она не существует
            string subjectFolder = System.IO.Path.Combine(_saveFolder, _currentSubject);
            if (!Directory.Exists(subjectFolder))
            {
                Directory.CreateDirectory(subjectFolder);
            }

            // Сохраняем тест в соответствующей папке
            string fileName = TestNameTextBox.Text.Trim() + ".json";
            string filePath = System.IO.Path.Combine(subjectFolder, fileName);

            string json = JsonConvert.SerializeObject(_questions, Formatting.Indented);
            File.WriteAllText(filePath, json);

            // Синхронизируем данные с Firebase (опционально)
            await SaveTestToFirebase(TestNameTextBox.Text.Trim());

            MessageBox.Show("Тест успешно сохранён и синхронизирован с Firebase!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            _isTestModified = false; // Сбрасываем флаг изменений
            AfterSaveUpdate();
        }






        private void AfterSaveUpdate()
        {
            QuestionsListBox.ItemsSource = null;
            QuestionsListBox.ItemsSource = _questions;

            if (_questions.Any())
            {
                QuestionsListBox.SelectedIndex = 0;
            }
            else
            {
                QuestionsListBox.SelectedIndex = -1;
            }
        }


        private void SavedTestsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SavedTestsListBox.SelectedItem is string selectedTest)
            {
                try
                {
                    // Формируем URL для выбранного теста
                    string url = $"{FirebaseBaseUrl}{_currentSubject}/{selectedTest}.json";
                    using (HttpClient client = new HttpClient())
                    {
                        var response = client.GetAsync(url).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string json = response.Content.ReadAsStringAsync().Result;

                            // Десериализуем массив вопросов
                            var loadedQuestions = JsonConvert.DeserializeObject<List<Question>>(json);

                            if (loadedQuestions != null && loadedQuestions.Any())
                            {
                                _questions = new ObservableCollection<Question>(loadedQuestions);
                                QuestionsListBox.ItemsSource = _questions;

                                // Отображаем первый вопрос
                                if (_questions.Any())
                                {
                                    _currentQuestionIndex = 0;
                                    LoadQuestionToEditor(_questions[0]);
                                }
                                else
                                {
                                    ClearEditor();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Файл пуст или повреждён.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                QuestionsListBox.ItemsSource = null;
                                ClearEditor();
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при загрузке теста: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }












        private ObservableCollection<Question> _questions = new ObservableCollection<Question>();
        private int _currentQuestionIndex = -1;
        public MainWindow()
        {
            InitializeComponent();
            QuestionsListBox.ItemsSource = _questions; // Привязка данных для списка вопросов

            SubjectLabel.Content = _currentSubject; // Устанавливаем текст при загрузке окна

        }



        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (child is T typedChild)
                    {
                        yield return typedChild;
                    }

                    if (child is DependencyObject depChild)
                    {
                        foreach (var childOfChild in FindLogicalChildren<T>(depChild))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }


    




        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && int.TryParse(radioButton.Tag.ToString(), out int points))
            {
                // Устанавливаем баллы в текущий вопрос
                if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
                {
                    _questions[_currentQuestionIndex].Points = points;
                }

                // Обновляем баллы в Firebase
                await UpdateQuestionPointsInFirebase();
            }
        }



        private async Task UpdateQuestionPointsInFirebase()
        {
            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count && !string.IsNullOrWhiteSpace(TestNameTextBox.Text))
            {
                var question = _questions[_currentQuestionIndex];
                string testName = TestNameTextBox.Text.Trim();

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string url = $"{FirebaseBaseUrl}{_teacherName}/{testName}/{_currentQuestionIndex}.json";
                        string json = JsonConvert.SerializeObject(question, Formatting.Indented);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PatchAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Ошибка при обновлении данных в Firebase: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при подключении к Firebase: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private async void RefreshSavedTestsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSavedTestsFromFirebase();
        }



        private async void LoadSavedTestsFromFirebaseButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSavedTestsFromFirebase(); // Вызов асинхронного метода
        }


        private async Task LoadSavedTestsFromFirebase()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Формируем URL для текущего предмета
                    string url = $"{FirebaseBaseUrl}{_currentSubject}.json";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        // Проверяем, пуст ли JSON
                        if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                        {
                            MessageBox.Show($"Для предмета '{_currentSubject}' нет сохраненных тестов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            SavedTestsListBox.ItemsSource = null; // Очищаем список тестов
                            QuestionsListBox.ItemsSource = null; // Очищаем вопросы
                            ClearEditor();
                            return;
                        }

                        // Десериализация тестов
                        var rawData = JsonConvert.DeserializeObject<Dictionary<string, List<Question>>>(json);

                        if (rawData != null && rawData.Any())
                        {
                            // Устанавливаем данные в список SavedTestsListBox
                            SavedTestsListBox.ItemsSource = rawData.Keys;
                        }
                        else
                        {
                            MessageBox.Show($"Для предмета '{_currentSubject}' нет сохраненных тестов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            SavedTestsListBox.ItemsSource = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при загрузке данных: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подключении к Firebase: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }







        private async Task SaveTestToFirebase(string testName)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Сохраняем тест в Firebase под текущим предметом
                    string url = $"{FirebaseBaseUrl}{_currentSubject}/{testName}.json";
                    string json = JsonConvert.SerializeObject(_questions, Formatting.Indented);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Тест успешно синхронизирован с Firebase!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при синхронизации: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подключении к Firebase: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void CreateNewTestButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, были ли изменения в текущем тесте
            if (_isTestModified)
            {
                var result = MessageBox.Show(
                    "Вы внесли изменения в текущий тест. Сохранить изменения?",
                    "Сохранение изменений",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentTest(); // Сохраняем текущий тест
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return; // Отменяем создание нового теста
                }
            }

            // Очищаем текущий список вопросов
            _questions.Clear();
            QuestionsListBox.ItemsSource = null;
            QuestionsListBox.ItemsSource = _questions;

            // Очищаем текстовое поле имени теста
            TestNameTextBox.Text = string.Empty;

            // Очищаем редактор
            ClearEditor();

            // Сбрасываем флаг изменений
            _isTestModified = false;

            MessageBox.Show("Новый тест создан. Введите имя теста и добавьте вопросы.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Открыть окно WindowTeacher
            var teacherWindow = new WindowTeacher();
            teacherWindow.Show();

            // Закрыть текущее окно
            this.Close();
        }

        // Метод для случайного перемешивания StackPanel
        private void ShuffleAnswersButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersPanel.Children.Count > 1)
            {
                var random = new Random();
                var shuffled = AnswersPanel.Children.Cast<UIElement>()
                                 .OrderBy(x => random.Next())
                                 .ToList();

                AnswersPanel.Children.Clear();
                foreach (var element in shuffled)
                {
                    AnswersPanel.Children.Add(element);
                }
            }
        }

        // Добавление нового вопроса
        private void CreateNewQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TestNameTextBox.Text))
            {
                MessageBox.Show("Введите имя теста перед добавлением вопроса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newQuestion = new Question
            {
              
                Text = "Сұрақ жазу",
                
              
                Points = 1,
                Answers = new ObservableCollection<Answer> { new Answer { Text = "Жауабы", IsCorrect = false } }
            };

        

            _questions.Add(newQuestion);
            QuestionsListBox.ItemsSource = null;
            QuestionsListBox.ItemsSource = _questions;
            QuestionsListBox.SelectedIndex = _questions.Count - 1;

            _isTestModified = true; // Устанавливаем флаг изменения
        }





        // Удаление вопроса
        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
            {
                _questions.RemoveAt(_currentQuestionIndex);

                if (_questions.Count > 0)
                {
                    // Устанавливаем новый текущий индекс
                    _currentQuestionIndex = Math.Min(_currentQuestionIndex, _questions.Count - 1);
                    LoadQuestionToEditor(_questions[_currentQuestionIndex]); // Загружаем новый вопрос
                }
                else
                {
                    // Если вопросов больше нет, очищаем интерфейс
                    _currentQuestionIndex = -1;
                    ClearEditor();
                }

                QuestionsListBox.ItemsSource = null;
                QuestionsListBox.ItemsSource = _questions;
                _isTestModified = true; // Отмечаем изменения
            }
            else
            {
                MessageBox.Show("Не выбран вопрос для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }




        private void ClearEditor()
        {
            QuestionTextBox.Clear(); // Очищаем текст вопроса
            AnswersPanel.Children.Clear(); // Очищаем панель с ответами
            _currentQuestionIndex = -1; // Сбрасываем индекс текущего вопроса
        }



        // Загрузка вопроса в редактор при выборе
        private void QuestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
            {
                SaveCurrentQuestion(); // Сохраняем текущий вопрос
            }

            _currentQuestionIndex = QuestionsListBox.SelectedIndex;

            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
            {
                LoadQuestionToEditor(_questions[_currentQuestionIndex]); // Загружаем выбранный вопрос
            }
            else
            {
                ClearEditor();
            }
        }



        private void LoadQuestionToEditor(Question question)
        {
            if (question == null)
            {
                ClearEditor();
                return;
            }

            // Загружаем текст вопроса
            QuestionTextBox.Text = question.Text;

            // Устанавливаем выбранный RadioButton на основе баллов
            foreach (var radioButton in FindLogicalChildren<RadioButton>(this))
            {
                if (radioButton.GroupName == "Scores" && int.TryParse(radioButton.Tag.ToString(), out int points))
                {
                    radioButton.IsChecked = (points == question.Points);
                }
            }

            // Очищаем панель ответов
            AnswersPanel.Children.Clear();

            // Добавляем ответы в интерфейс
            foreach (var answer in question.Answers)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var textBox = new TextBox
                {
                    Width = 300,
                    Height = 35,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,

                    Text = answer.Text
                };

                var checkBox = new CheckBox
                {
                    IsChecked = answer.IsCorrect,
                    LayoutTransform = new ScaleTransform
                    {
                        ScaleX = 2,
                        ScaleY = 2
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                stackPanel.Children.Add(checkBox);
                stackPanel.Children.Add(textBox);

                AnswersPanel.Children.Add(stackPanel);
            }
        }







        // Убираем RadioButton, оставляем только текстовые поля и галочки для ответа
        private void SaveCurrentQuestion()
        {
            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
            {
                var question = _questions[_currentQuestionIndex];
                question.Text = QuestionTextBox.Text;

                // Обновляем ответы
                question.Answers = new ObservableCollection<Answer>(
                    AnswersPanel.Children.OfType<StackPanel>().Select(panel =>
                    {
                        var textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                        var checkBox = panel.Children.OfType<CheckBox>().FirstOrDefault();
                        return new Answer
                        {
                            Text = textBox?.Text ?? string.Empty,
                            IsCorrect = checkBox?.IsChecked ?? false
                        };
                    }));
            }
        }






        // Обработка изменения текста вопроса
        private void QuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
            {
                _questions[_currentQuestionIndex].Text = QuestionTextBox.Text;
                _isTestModified = true; // Устанавливаем флаг изменения
            }
        }





        // Добавление варианта ответа
        private void AddAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var checkBox = new CheckBox
            {
                LayoutTransform = new ScaleTransform
                {
                    ScaleX = 2,
                    ScaleY = 2
                },
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var textBox = new TextBox
            {
                Width = 290,
                Height = 35,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 10, 0),
                Text = "Жауабы",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

          

            var deleteButton = new Button
            {
                Width = 40,
                Height = 40,
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                Content = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/del.png")),
                    Width = 30,
                    Height = 30
                },
                Style = (Style)FindResource("CustomButtonStyle1")
            };

            deleteButton.Click += (s, args) =>
            {
                if (AnswersPanel.Children.Contains(stackPanel))
                {
                    AnswersPanel.Children.Remove(stackPanel);
                }
            };

            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(deleteButton);

            AnswersPanel.Children.Add(stackPanel);
        }

        public MainWindow(string selectedSubject)
        {
            InitializeComponent();
            _currentSubject = selectedSubject;
            SubjectLabel.Content = _currentSubject;
        }





        // Сохранение теста в JSON
        private void SaveToJsonButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentTest(); // Сохраняем текущий тест
        }


        // Загрузка теста из JSON
      
    }

    public class Question : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public int Points { get; set; }
        public ObservableCollection<Answer> Answers { get; set; } = new ObservableCollection<Answer>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Answer
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}