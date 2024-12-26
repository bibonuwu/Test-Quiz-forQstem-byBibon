using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            foreach (var answer in question.Answers)
            {
                var checkBox = new CheckBox
                {
                    Content = answer.Text,
                    Tag = answer.IsCorrect,
                    FontSize = 16
                };
                AnswersPanel.Children.Add(checkBox);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAnswers = AnswersPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .ToList();

            var correctAnswers = _questions[_currentQuestionIndex].Answers.Where(a => a.IsCorrect).ToList();

            // Подсчет баллов
            int questionScore = CalculateScore(selectedAnswers, correctAnswers);
            _score += questionScore;

            _currentQuestionIndex++;

            if (_currentQuestionIndex < _questions.Count)
            {
                LoadQuestion(_questions[_currentQuestionIndex]);
            }
            else
            {
                MessageBox.Show($"Тест завершён! Ваш результат: {_score} из {_questions.Count * 2}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private int CalculateScore(List<CheckBox> selectedAnswers, List<Answer> correctAnswers)
        {
            int correctCount = selectedAnswers.Count(cb => (bool)cb.Tag);
            int incorrectCount = selectedAnswers.Count(cb => !(bool)cb.Tag);

            if (correctAnswers.Count == 1)
            {
                if (correctCount == 1 && incorrectCount == 0) return 2; // Один правильный ответ
                if (correctCount == 1 && incorrectCount > 0) return 1; // Один правильный и один неправильный
                return 0; // Иначе
            }
            else if (correctAnswers.Count > 1)
            {
                if (correctCount == correctAnswers.Count && incorrectCount == 0) return 2; // Все правильные без ошибок
                if (correctCount > 0 && incorrectCount == 0) return 1; // Часть правильных без ошибок
                return 0; // Иначе
            }

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
