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


        public MainWindow(string teacherName)
        {
            InitializeComponent();
            _teacherName = teacherName;

            // Устанавливаем папку для сохранения тестов
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string baseFolder = System.IO.Path.Combine(desktopPath, "Тест сұрақтары QSTEM");
            _saveFolder = System.IO.Path.Combine(baseFolder, _teacherName);

            InitializeFolders(); // Создаём папки
            LoadSavedTests(); // Загружаем список тестов

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
                var files = Directory.GetFiles(_saveFolder, "*.json");
                SavedTestsListBox.ItemsSource = files.Select(System.IO.Path.GetFileName).ToList();
            }
        }
        private void SaveCurrentTest()
        {
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

            string fileName = TestNameTextBox.Text.Trim() + ".json";
            string filePath = System.IO.Path.Combine(_saveFolder, fileName);

            string json = JsonConvert.SerializeObject(_questions, Formatting.Indented);
            File.WriteAllText(filePath, json);

            MessageBox.Show("Тест успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            _isTestModified = false; // Сбрасываем флаг изменений

            AfterSaveUpdate(); // Синхронизируем интерфейс
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
                    SavedTestsListBox.SelectedItem = null; // Сбрасываем выбор
                    return;
                }
            }

            if (SavedTestsListBox.SelectedItem is string selectedFile)
            {
                string filePath = System.IO.Path.Combine(_saveFolder, selectedFile);

                if (File.Exists(filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        var loadedQuestions = JsonConvert.DeserializeObject<ObservableCollection<Question>>(json);

                        if (loadedQuestions == null || !loadedQuestions.Any())
                        {
                            MessageBox.Show("Файл не содержит вопросов или повреждён.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            QuestionsListBox.ItemsSource = null;
                            return;
                        }

                        _questions = loadedQuestions;
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

                        TestNameTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(selectedFile);
                        _isTestModified = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }







        private void RefreshSavedTestsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSavedTests();
        }



        private ObservableCollection<Question> _questions = new ObservableCollection<Question>();
        private int _currentQuestionIndex = -1;
        public MainWindow()
        {
            InitializeComponent();
            QuestionsListBox.ItemsSource = _questions; // Привязка данных для списка вопросов


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
                Text = "Новый вопрос",
                Points = 1,
                Answers = new ObservableCollection<Answer> { new Answer { Text = "Ответ 1", IsCorrect = false } }
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
                    _currentQuestionIndex = Math.Min(_currentQuestionIndex, _questions.Count - 1);
                    LoadQuestionToEditor(_questions[_currentQuestionIndex]);
                }
                else
                {
                    _currentQuestionIndex = -1;
                    ClearEditor();
                }
            }
        }


        // Очистка редактора
        private void ClearEditor()
        {
            QuestionTextBox.Clear();
            PointsTextBox.Clear();
            AnswersPanel.Children.Clear();
            _currentQuestionIndex = -1;
        }

        // Загрузка вопроса в редактор при выборе
        private void QuestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuestionsListBox.SelectedIndex >= 0 && QuestionsListBox.SelectedIndex < _questions.Count)
            {
                SaveCurrentQuestion(); // Сохраняем текущий вопрос перед переключением
                _currentQuestionIndex = QuestionsListBox.SelectedIndex;
                LoadQuestionToEditor(_questions[_currentQuestionIndex]);
            }
        }

        private void LoadQuestionToEditor(Question question)
        {
            if (question == null)
            {
                MessageBox.Show("Вопрос не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            QuestionTextBox.Text = question.Text;
            PointsTextBox.Text = question.Points.ToString();
            AnswersPanel.Children.Clear();

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
                    Margin = new Thickness(0, 0, 10, 0),
                    Text = answer.Text
                };

                var checkBox = new CheckBox
                {
                    Content = "Верно",
                    IsChecked = answer.IsCorrect,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(checkBox);

                AnswersPanel.Children.Add(stackPanel);
            }
        }



        // Сохранение текущего вопроса
        private void SaveCurrentQuestion()
        {
            if (_currentQuestionIndex >= 0)
            {
                var question = _questions[_currentQuestionIndex];
                question.Text = QuestionTextBox.Text;
                question.Points = int.TryParse(PointsTextBox.Text, out var points) ? points : 1;
                question.Answers = new ObservableCollection<Answer>(AnswersPanel.Children.OfType<StackPanel>().Select(panel =>
                {
                    var textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                    var checkBox = panel.Children.OfType<CheckBox>().FirstOrDefault();
                    return new Answer
                    {
                        Text = textBox?.Text ?? "",
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

            var textBox = new TextBox
            {
                Width = 300,
                Margin = new Thickness(0, 0, 10, 0),
                Text = "Введите ответ"
            };

            var checkBox = new CheckBox
            {
                Content = "Верно",
                Margin = new Thickness(0, 0, 10, 0)
            };

            var deleteButton = new Button
            {
                Content = "Удалить",
                Width = 70
            };

            // Добавляем обработчик на кнопку "Удалить"
            deleteButton.Click += (s, args) =>
            {
                // Удаляем stackPanel из AnswersPanel, если он существует
                if (AnswersPanel.Children.Contains(stackPanel))
                {
                    AnswersPanel.Children.Remove(stackPanel);
                }
            };

            // Добавляем элементы в StackPanel
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(deleteButton);

            // Добавляем StackPanel в AnswersPanel
            AnswersPanel.Children.Add(stackPanel);
        }





        // Сохранение теста в JSON
        private void SaveToJsonButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentTest(); // Сохраняем текущий тест
        }


        // Загрузка теста из JSON
        private void LoadFromJsonButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string json = System.IO.File.ReadAllText(openFileDialog.FileName);

                // Десериализуем вопросы
                _questions = JsonConvert.DeserializeObject<ObservableCollection<Question>>(json)
                             ?? new ObservableCollection<Question>();

                QuestionsListBox.ItemsSource = _questions;

                if (_questions.Any())
                {
                    _currentQuestionIndex = 0;
                    LoadQuestionToEditor(_questions[_currentQuestionIndex]);
                }
                else
                {
                    _currentQuestionIndex = -1;
                    ClearEditor();
                }
            }
        }

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

