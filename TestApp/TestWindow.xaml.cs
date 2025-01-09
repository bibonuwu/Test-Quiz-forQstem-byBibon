using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestApp
{
    public partial class TestWindow : Window
    {
        private readonly List<Question> _questions;
        private int _currentQuestionIndex = 0;
        private int _score = 0;


        public TestWindow(string testName, List<Question> questions)
        {
            InitializeComponent();
            Title = $"Тест: {testName}";
            _questions = questions;
            LoadQuestion(_questions[_currentQuestionIndex]);
        }

        private void LoadQuestion(Question question)
        {
            QuestionTextBlock.Text = question.Text;
            AnswersPanel.Children.Clear();

            int correctAnswerCount = question.Answers.Count(a => a.IsCorrect);

            if (correctAnswerCount == 1)
            {
                var groupName = $"Question{_currentQuestionIndex}";

                foreach (var answer in question.Answers)
                {
                    var radioButton = new RadioButton
                    {
                        Content = answer.Text,
                        Tag = answer.IsCorrect,
                        GroupName = groupName,
                        FontSize = 16,
                        Background = Brushes.Transparent,
                        Style = (Style)FindResource("CustomRadioButtonStyle")
                    };

                    AnswersPanel.Children.Add(radioButton);
                }
            }
            else
            {
                foreach (var answer in question.Answers)
                {
                    var checkBox = new CheckBox
                    {
                        Content = answer.Text,
                        Tag = answer.IsCorrect,
                        FontSize = 16,
                        Background = Brushes.Transparent,
                        Style = (Style)FindResource("CustomCheckBoxStyle")
                    };

                    // Подписываемся на события Checked и Unchecked
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Unchecked;

                    AnswersPanel.Children.Add(checkBox);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var selectedCount = AnswersPanel.Children
                .OfType<CheckBox>()
                .Count(cb => cb.IsChecked == true);

            if (selectedCount > 3)
            {
                // Если выбрано больше трех ответов, отменяем выбор текущего CheckBox
                var checkBox = sender as CheckBox;
                if (checkBox != null)
                {
                    checkBox.IsChecked = false;
                    MessageBox.Show("Вы можете выбрать только три ответа.", "Ограничение выбора", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить дополнительную логику, если необходимо
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            List<bool> selectedCorrectAnswers = new List<bool>();

            if (_questions[_currentQuestionIndex].Answers.Count(a => a.IsCorrect) == 1)
            {
                // Обработка для RadioButton
                var selectedRadioButton = AnswersPanel.Children
                    .OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked == true);

                if (selectedRadioButton != null)
                {
                    selectedCorrectAnswers.Add((bool)selectedRadioButton.Tag);
                }
            }
            else
            {
                // Обработка для CheckBox с ограничением в 3 ответа
                var selectedCheckBoxes = AnswersPanel.Children
                    .OfType<CheckBox>()
                    .Where(cb => cb.IsChecked == true)
                    .Take(3) // Берем только первые три выбранных
                    .ToList();

                if (selectedCheckBoxes.Count > 3)
                {
                    MessageBox.Show("Вы можете выбрать только три ответа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selectedCorrectAnswers.AddRange(selectedCheckBoxes.Select(cb => (bool)cb.Tag));
            }

            var correctAnswers = _questions[_currentQuestionIndex].Answers.Where(a => a.IsCorrect).ToList();

            // Подсчет баллов
            int questionScore = CalculateScore(selectedCorrectAnswers, correctAnswers);
            _score += questionScore;

            _currentQuestionIndex++;

            if (_currentQuestionIndex < _questions.Count)
            {
                LoadQuestion(_questions[_currentQuestionIndex]);
            }
            else
            {
                int maxScore = _questions.Sum(q => q.Answers.Count(a => a.IsCorrect));
                MessageBox.Show($"Тест завершён! Ваш результат: {_score} из {maxScore}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private int CalculateScore(List<bool> selectedCorrectAnswers, List<Answer> correctAnswers)
        {
            int correctCount = selectedCorrectAnswers.Count(isCorrect => isCorrect);
            int incorrectCount = selectedCorrectAnswers.Count - correctCount;

            // Условие 1: Все три ответа правильные
            if (correctCount == 3 && incorrectCount == 0)
            {
                return 2; // x+x+x=2
            }

            // Условие 2: Два правильных ответа и один неправильный
            if (correctCount == 2 && incorrectCount == 1)
            {
                return 1; // x+x+y=1
            }

            // Условие 3: Один правильный ответ и два неправильных
            if (correctCount == 1 && incorrectCount == 2)
            {
                return 0; // x+y+y=0
            }

            // Условие 4: Все три ответа неправильные
            if (correctCount == 0 && incorrectCount == 3)
            {
                return 0; // y+y+y=0
            }

            // Если выбрано меньше или больше трех ответов, начисление баллов не производится
            return 0;
        }











    }

    public class TestQuestion
    {
        public string Text { get; set; }
        public List<TestAnswer> Answers { get; set; }
    }

    public class TestAnswer
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

}
