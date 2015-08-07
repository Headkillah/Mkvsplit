using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace mkvsplit
{
    public partial class RangeDispalyControl
	{

        public string StartTime;
        public string FinishTime;
        public byte Number;
        public bool Additable;




        /// <summary> конструктор, формирует свойства временного блока </summary>
        public RangeDispalyControl(string startTime, string finishTime, byte number, bool additable)
		{
            InitializeComponent();

		    StartTime = startTime;
		    FinishTime = finishTime;
            Number = number;
		    Additable = additable;

            LabelNumber.Content = "Range №" + number;
		    TextBoxStartTime.Text = startTime;
		    TextBoxFinishTime.Text = finishTime;
            if (Additable) AddSign.Visibility = Visibility.Visible;
		}

        private void TextBoxTime_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Background = Brushes.WhiteSmoke;
            ((TextBox)sender).Foreground = Brushes.Black;
        }

        /// <summary> проверка текста в блоке на предмет форматирования времени </summary>
        private void TextBoxTime_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Background = null;
            ((TextBox)sender).Foreground = Brushes.WhiteSmoke;

           
            
            //вычленяем имя текстблока вызвавшего событие
            var name = ((TextBox)sender).Name;

            //вычленяем текст из текстблока вызвавшего событие
            var text = ((TextBox)sender).Text;

            //создаём подстроки для часов минут и секунд и миллисекунд
            string hours, minutes, seconds, miliseconds;

            //пытаемся интерпретировать строку
            if (ParsingTime(text, out hours, out minutes, out seconds, out miliseconds))
            {//если удаётся конвертируем подстроки во временной формат типа 00:00:00.000
                if (ConvertingTime(hours, minutes, seconds, miliseconds, ref text))
                {
                    // если конвертирование удачно записываем форматрованное время обратно в текстовый блок
                    ((TextBox)sender).Text = text;
                    
                    //придаём блоку белый цвет одобрения времени
                    //((TextBox)sender).Background = new SolidColorBrush(Colors.White);

                    //узнаём какой блок вызвал событие и записываем информатицию из него соотвественно
                    //либо как стартовое время, либо как финишное
                    switch (name)
                    {
                        case "TextBoxStartTime":
                            StartTime = text;
                            break;

                        case "TextBoxFinishTime":
                            FinishTime = text;
                            break;
                    }
                }
                //если конвертирование не  удалось 
                else WrongTimeFormatSignal(sender);

            }
                // если формат времени не распознан
            else WrongTimeFormatSignal(sender);
        }

        private void ButtonDeleteRange_Click(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < MainWindow.ListOfRanges.Count; i++)
            {
                if (MainWindow.ListOfRanges[i].Number != Number) continue;

                MainWindow.ListOfRanges.RemoveAt(i);

                MainWindow._wm._taskManager.Add(null,
                    delegate { MainWindow._wm.RefreshRanges(); });

                return;
            }
        }

        private void ButtonMoveRangeUp_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.ListOfRanges.Count < 2) return;

            for (var i = 0; i < MainWindow.ListOfRanges.Count; i++)
            {
                if (MainWindow.ListOfRanges[i].Number != Number) continue;

                if (i == 0) return;
                var movementrange = MainWindow.ListOfRanges[i];

                MainWindow.ListOfRanges.RemoveAt(i);
                MainWindow.ListOfRanges.Insert(i - 1, movementrange);

                MainWindow._wm._taskManager.Add(null,
                    delegate { MainWindow._wm.RefreshRanges(); });

                return;
            }
        }

        private void ButtonMoveRangeDown_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.ListOfRanges.Count < 2) return;

            for (var i = 0; i < MainWindow.ListOfRanges.Count; i++)
            {
                if (MainWindow.ListOfRanges[i].Number != Number) continue;

                if (i == MainWindow.ListOfRanges.Count - 1) return;

                var movementrange = MainWindow.ListOfRanges[i];

                MainWindow.ListOfRanges.RemoveAt(i);
                MainWindow.ListOfRanges.Insert(i + 1, movementrange);

                MainWindow._wm._taskManager.Add(null,
                    delegate { MainWindow._wm.RefreshRanges(); });

                return;
            }

        }

        private void GrabStartTimeFromPlayerButtonClick(object sender, RoutedEventArgs e)
        {
            var filename = Path.GetFileName(MainWindow._wm.TextBoxPath.Text);

            TextBoxStartTime.Text = MPC.GetTimeFromMPC(filename);

            //MainWindow._wm.
                TextBoxTime_LostFocus(TextBoxStartTime, null);
            
        }

        private void GrabFinishTimeFromPlayerButtonClick(object sender, RoutedEventArgs e)
        {
            var filename = Path.GetFileName(MainWindow._wm.TextBoxPath.Text);

            TextBoxFinishTime.Text = MPC.GetTimeFromMPC(filename);

            //MainWindow._wm.
            TextBoxTime_LostFocus(TextBoxFinishTime, null);
        }

        /// <summary> расковыривание строки времени из блока на составные строки содержащие часы, минуты, секунды и миллисекнуды </summary>
        private static bool ParsingTime(string text, out string hours, out string minutes, out string seconds, out string miliseconds)
        {

            if (Regexes.Fulltimeformat.IsMatch(text))
            {
                var positions = text.Split(':');

                hours = positions[0];
                minutes = positions[1];

                positions = positions[2].Split('.');

                seconds = positions[0];
                miliseconds = positions[1];

                return true;
            }

            if (Regexes.Playerformat.IsMatch(text))
            {
                var positions = text.Split(':');

                hours = "00";

                minutes = positions[0];

                positions = positions[1].Split('.');

                seconds = positions[0];
                miliseconds = positions[1];
                return true;
            }

            if (Regexes.Simpleformat.IsMatch(text))
            {
                var positions = text.Split(':');

                hours = positions[0];
                minutes = positions[1];
                seconds = positions[2];
                miliseconds = "000";
                return true;


            }

            if (Regexes.Simplestformat.IsMatch(text))
            {
                var positions = text.Split(':');

                hours = "00";
                minutes = positions[0];
                seconds = positions[1];
                miliseconds = "000";
                return true;
            }

            hours = "";
            minutes = "";
            seconds = "";
            miliseconds = "";
            return false;

        }

        /// <summary> сигнализация, что формат времени не верен </summary>
        private static void WrongTimeFormatSignal(object sender)
        {
            ((TextBox)sender).Background = new SolidColorBrush(Colors.Red);
        }

        /// <summary> конвертирует строки содержащие количество часов минут и миллисекунд в текст вида 00:00:00.000 с проверкой на коректность чисел </summary>
        private static bool ConvertingTime(string stringHours, string stringMinutes, string stringSeconds, string stringMiliseconds, ref string text)
        {
            if (stringHours == null) throw new ArgumentNullException("stringHours");
            if (stringMinutes == null) throw new ArgumentNullException("stringMinutes");
            if (stringSeconds == null) throw new ArgumentNullException("stringSeconds");
            if (stringMiliseconds == null) throw new ArgumentNullException("stringMiliseconds");

            short hours;
            short.TryParse(stringHours, out hours);


            short minutes;
            short.TryParse(stringMinutes, out minutes);


            short seconds;
            short.TryParse(stringSeconds, out seconds);


            short miliseconds;
            short.TryParse(stringMiliseconds, out miliseconds);

            if (hours > 23 | minutes > 59 | seconds > 59 | miliseconds > 999) return false;

            text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00") + "." +
                   miliseconds.ToString("000");
            return true;
        }

        /// <summary> класс экземпляров регулярных выражений </summary>
        public static class Regexes
        {
            public static readonly Regex Fulltimeformat = new Regex(@"^\d?\d:\d?\d:\d?\d\.\d?\d?\d$", RegexOptions.Compiled);

            public static readonly Regex Playerformat = new Regex(@"^\d?\d:\d?\d\.\d?\d?\d$", RegexOptions.Compiled);
            public static readonly Regex Simpleformat = new Regex(@"^\d?\d:\d?\d:\d?\d$", RegexOptions.Compiled);
            public static readonly Regex Simplestformat = new Regex(@"^\d?\d:\d?\d$", RegexOptions.Compiled);
        }
	}
}