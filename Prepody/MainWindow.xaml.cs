using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Globalization;

namespace Prepody
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=C:\Users\тёма\Downloads\Students.db;";

        public MainWindow()
        {
            InitializeComponent();
            // "Подписываемся" на событие активации окна
            Activated += MainWindow_Activated;
            // "Подписываемся" на событие деактивации окна
            Deactivated += MainWindow_Deactivated;
            LoadDataFromDatabase();
            // Добавление обработчика события для кнопки сохранения изменений

        }
        private void LoadDataFromDatabase()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Students_Note";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    SQLiteDataReader reader = command.ExecuteReader();

                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            FullName = reader["FullName"].ToString(),
                            GroupName = reader["GroupName"].ToString(),
                            NeedsReassessment = Convert.ToBoolean(reader["NeedsReassessment"])
                        };
                        students.Add(student);
                    }

                    // Отображение данных в WPF окне
                    studentsListView.ItemsSource = students;
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных из базы данных: {ex.Message}");
            }
        }

        public class BoolToYesNoConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool)
                {
                    bool boolValue = (bool)value;
                    return boolValue ? "Да" : "Нет";
                }

                return "Нет";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class Student
        {
            public string FullName { get; set; }
            public string GroupName { get; set; }
            public bool NeedsReassessment { get; set; }
            public bool IsApproved { get; set; }
            public string DateReAt { get; set; }

        }

        private void SaveChangesToDatabase(Student student)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Students_Note SET NeedsReassessment = @NeedsReassessment WHERE FullName = @FullName";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@NeedsReassessment", student.NeedsReassessment ? 1 : 0);
                    command.Parameters.AddWithValue("@FullName", student.FullName);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes to the database: {ex.Message}");
            }
        }

        // Обработчик события нажатия на кнопку сохранения изменений
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента (пример)
            Student selectedStudent = (Student)studentsListView.SelectedItem;

            // Проверяем, что студент выбран
            if (selectedStudent != null)
            {
                // Сохраняем изменения в базе данных
                SaveChangesToDatabase(selectedStudent);
                MessageBox.Show("Изменения сохранены в базе данных.");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента для сохранения изменений.");
            }
        }

        // Обработчик события нажатия на кнопку обновления данных
        private void RefreshDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Загрузка данных из базы данных
            LoadDataFromDatabase();
            MessageBox.Show("Данные успешно обновлены.");
        }

        //Кнопка сворачивания приложения в трей
        private void MinButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Создаем анимацию для плавного сворачивания
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1.0; // начальное значение прозрачности
            animation.To = 0.0; // конечное значение прозрачности
            animation.Duration = TimeSpan.FromSeconds(0.5); // продолжительность анимации

            // Привязываем анимацию к окну
            this.BeginAnimation(Window.OpacityProperty, animation);

            // Установим задержку для плавного сворачивания окна
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5); // задержка должна быть равна продолжительности анимации
            timer.Tick += (s, ea) =>
            {
                // Сворачиваем окно
                this.WindowState = WindowState.Minimized;
                timer.Stop(); // останавливаем таймер после сворачивания окна
            };
            timer.Start();
        }
             private void MainWindow_Activated(object sender, EventArgs e)
        {
            // При активации окна отменяем затемнение
            this.BeginAnimation(Window.OpacityProperty, null);
            this.Opacity = 1.0; // устанавливаем полную прозрачность
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            // Создаем анимацию для плавного изменения прозрачности
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1.0; // начальное значение прозрачности
            animation.To = 0.7; // конечное значение прозрачности
            animation.Duration = TimeSpan.FromSeconds(0.5); // продолжительность анимации

            // Привязываем анимацию к окну
            this.BeginAnimation(Window.OpacityProperty, animation);
        }

        //Кнопка закрытия приложения
        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


    }
}

