using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Path = System.IO.Path;
using File = System.IO.File;
using TextBox = System.Windows.Controls.TextBox;


namespace mkvsplit
{

    public partial class MainWindow
    {
        /// <summary> диспетчер </summary>
        public TaskManager _taskManager;

        /// <summary> глобальный список объектов типа RangeDispalyControl  </summary>
        public static List<RangeDispalyControl> ListOfRanges = new List<RangeDispalyControl>();
 


        // Объект типа АКНО
        public static MainWindow _wm;

        public MainWindow()
        {
            InitializeComponent();

            //задаём событие при выходе программы
            Application.Current.Exit += OnAppExit;

            //создание статической ссылки на объект MainWindow 
            _wm = this;

            //создаём и запускаем диспетчер выполнения заданий в параллельнм потоке
            _taskManager = new TaskManager();


            //разрешаем обработку события Droр для текстового блока с путём к файлу
            TextBoxPath.AllowDrop = true;

            //разрешаем обработку события Droр для текстового блока с путём к консольному приложеню
            TextBoxMmgPath.AllowDrop = true;
            
            //задаём максимальную высоту  ячейки с промежутками времени
            RangesScrollViewer.MaxHeight = Screen.PrimaryScreen.WorkingArea.Height - 200;
        }

        /// <summary> команда разбивки </summary>
        private void ButtonSplit_Click(object sender, RoutedEventArgs e)
        {
            

            // выключаем кнопку во избежание случайного повторного запуска
            //ButtonSplit.IsEnabled = false;
            //очищем окно лога
            TextBoxLog.Clear();

            // достаём из текстовых блоков пути к файлу и консольному приложению
            var processpath = TextBoxMmgPath.Text;
            var inputpath = TextBoxPath.Text;

            //проверяем адекватность текста в в блоках
            if (CheckingFileExisting(processpath, "mkvmerge.exe is not found.")) return;
            if (CheckingFileExisting(inputpath, "Mediafile is not found")) return;

            // задаём название будущего файла 
            var outputpath = GeneratingFileName(inputpath);

            


            // формируем строку аргументов для командной строки консольного приложения
            var argumentsstring = " -o " +
                                  "\"" + outputpath + "\" " +
                                  "\"" + inputpath + "\" " +
                                  MakingArgs();

            //MessageBox.Show(argumentsstring);

            // запуск консольного приложения
            consoleinterface.Start(processpath, argumentsstring, false);


        }

        /// <summary> создание строки параметров для mkv </summary>
        public string MakingArgs()
        {
            // "--split" "parts:00:00:00-00:01:00"  
            if (ListOfRanges.Count < 1) return "";

            var timesString = "\"--split\" \"parts:";
            
            foreach (var t in ListOfRanges)
            {
                if (t.Additable)
                {
                    timesString = timesString + "+";
                }
                timesString = timesString + t.StartTime + "-" + t.FinishTime + ",";
            }
            char[] trims = {',', ' '};

            return timesString.TrimEnd(trims)+'"';
            
        }
        
        /// <summary> открытие медиафайла </summary>
        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            //создаём диалог открытия файла
            var openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter =
                //характеристики типов файлов 
                    @"Все файлы|*.*|Все поддерживаемые форматы|*.ac3;*.eac3;*.aac;*.m4a;*.mp4;*.264;*.avc;" +
                    @"*.h264;*.x264;*.avi;*.drc;*.thd;*.thd+ac3;*.truehd;*.true-hd;*.dts;*.dtshd;*.dts-hd;*.flac;" +
                    @"*.ogg;*.ivf;*.mp4;*.m4v;*.mp2;*.mp3[;*.mpg;*.mpeg;*.m2v;*.mpv;*.evo;*.evob;*.vob];*.ts;*.m2ts;" +
                    @"*.mts;*.m1v;*.m2v;*.mpv[;*.mka;*.mks;*.mkv;*.mk3d;*.webm;*.webmv;*.webma];*.sup;*.mov;*.ogg;*.ogm;" +
                    @"*.ogv[;*.ra;*.ram;*.rm;*.rmvb;*.rv];*.srt;*.ass;*.ssa;*.tta;*.usf;*.xml;*.vc1;*.btn;*.idx;*.wav;*.wv;*.webm;*.webmv;*.webma|" +
                    @"A/52 (он же AC3) |*.ac3 *.eac3|AAC (Advanced Audio Coding) |*.aac *.m4a*.mp4|" +
                    @"элементарные потоки AVC/H.264 |*.264 *.avc*.h264*.x264|AVI (Audio/Video Interleaved) |*.avi|" +
                    @"Dirac |*.drc|Dolby TrueHD |*.thd *.thd+ac3*.truehd*.true-hd|" +
                    @"DTS/DTS-HD (Digital Theater System) |*.dts *.dtshd*.dts-hd|" +
                    @"FLAC (Free Lossless Audio Codec) |*.flac *.ogg|Видеофайлы IVF с VP8 |*.ivf|Файлы MP4 Audio/Video |*.mp4 *.m4v|" +
                    @"Файлы MPEG Audio |*.mp2 *.mp3|Программные потоки MPEG |[*.mpg *.mpeg *.m2v *.mpv *.evo *.evob *.vob]|" +
                    @"Транспортные потоки MPEG |*.ts *.m2ts*.mts|Элементарные потоки MPEG Video |*.m1v *.m2v*.mpv|" +
                    @"Файлы Matroska Audio/Video |[*.mka *.mks *.mkv *.mk3d *.webm *.webmv *.webma]|" +
                    @"Субтитры PGS/SUP |*.sup|Файлы QuickTime Audio/Video |*.mov|" +
                    @"Файлы Ogg/OGM Audio/Video |*.ogg *.ogm*.ogv|Файлы RealMedia Audio/Video |*.ra *.ram *.rm *.rmvb *.rv|" +
                    @"Текстовые субтитры SRT |*.srt|Текстовые субтитры SSA/ASS |*.ass *.ssa|TTA (The lossless True Audio codec) |*.tta|" +
                    @"Текстовые субтитры USF |*.usf *.xml|" +
                    @"Элементарные потоки VC1 |*.vc1|VobButtons |*.btn|Субтитры VobSub |*.idx|" +
                    @"WAVE (несжатое аудио PCM) |*.wav|" +
                    @"Аудио WAVPACK v4 |*.wv|Файла WebM Audio/Video |*.webm *.webmv*.webma",
                FilterIndex = 2, //задаём порядковый номер типа файла в вышеозначенном в списке по умолчанию
                RestoreDirectory = true
            };

            //проверяем закончилась ли успешно работа с диалогом открытия медиафайла и возвращаем в текстовый блок путь к файлу
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxPath.Text = openFileDialog1.FileName;
            }
        }

        /// <summary> запуск медиафайла </summary>
        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            var video = new Process
            {
                StartInfo = {
                    FileName = TextBoxPath.Text//, 
                    //Arguments = "ProcessStart.cs"
                }
            };
            try
            {
                video.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        /// <summary> открытие медиафайла </summary>
        private void ButtonMmgPathOpen_Click(object sender, RoutedEventArgs e)
        {

            var openFileDialog1 = new OpenFileDialog
            {
                //InitialDirectory = "c:\\",
                Filter = @"Все файлы|*.*|mkvmerge.exe|mkvmerge.exe",
                FilterIndex = 2,
                RestoreDirectory = true
            };


            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxMmgPath.Text = openFileDialog1.FileName;
            }
        }

        /// <summary> костыль для объяснения textbox'у, что курсор в него въехал (причина не выяснена) </summary>
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary> добавление пути к файлу в textbox через драгэндроп </summary>
        private void TextBox_Drop(object sender, DragEventArgs e)
        {
            //проверяем является ли информаци в буфере обмена ссылкой на файл
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            // вычленяем из списка файлов привнесённых при помощи драгэндроп массив строк с путями к эти м файлам
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            //берём первый путь и помещяем его в текстовый блок
            ((TextBox) sender).Text = files[0];


        }

        /// <summary> обработка вывода </summary>
        public void OnProcessExited(object sender, EventArgs eventArgs)
        {
            _taskManager.Add(null,
                delegate
                {
                    //включаем кнопку обратно
                    ButtonSplit.IsEnabled = true;

                    // добавляем в очередь задание на вывод сообщения о неверном времени
                    if (TextBoxLog.Text.Contains("The end time must be bigger"))
                    {
                        MessageBox.Show("The end time must be bigger than the start time.");
                    }


                    // добавляем в очередь задание на вывод сообщения о некорректном типе медиафайла

                    if (TextBoxLog.Text.Contains("has unknown type.") ||
                        TextBoxLog.Text.Contains("your file type is supported"))
                    {
                        MessageBox.Show("Mediafile has unknown type.\n" +
                                        "Supported file types:\n  A/52 (aka AC3) [ac3 eac3]\n  AAC (Advanced Audio Coding) [aac m4a mp4]\n  AVC/h.264 elementary streams [264 avc h264 x264]\n  AVI (Audio/Video Interleaved) [avi]\n  ALAC (Apple Lossless Audio Codec) [caf m4a mp4]\n  Dirac [drc]\n  Dolby TrueHD [thd thd+ac3 truehd true-hd]\n  DTS/DTS-HD (Digital Theater System) [dts dtshd dts-hd]\n  FLAC (Free Lossless Audio Codec) [flac ogg]\n  FLV (Flash Video) [flv]\n  HEVC/h.265 elementary streams [265 hevc h265 x265]\n  IVF with VP8 video files [ivf]\n  MP4 audio/video files [mp4 m4v]\n  MPEG audio files [mp2 mp3]\n  MPEG program streams [mpg mpeg m2v mpv evo evob vob]\n  MPEG transport streams [ts m2ts mts]\n  MPEG video elementary streams [m1v m2v mpv]\n  MPLS Blu-ray playlist [mpls]\n  Matroska audio/video files [mka mks mkv mk3d webm webmv webma]\n  PGS/SUP subtitles [sup]\n  QuickTime audio/video files [mov]\n  Ogg/OGM audio/video files [ogg ogm ogv]\n  Opus (in Ogg) audio files [opus ogg]\n  RealMedia audio/video files [ra ram rm rmvb rv]\n  SRT text subtitles [srt]\n  SSA/ASS text subtitles [ass ssa]\n  TTA (The lossless True Audio codec) [tta]\n  USF text subtitles [usf xml]\n  VC1 elementary streams [vc1]\n  VobButtons [btn]\n  VobSub subtitles [idx]\n  WAVE (uncompressed PCM audio) [wav]\n  WAVPACK v4 audio [wv]\n  WebM audio/video files [webm webmv webma]");
                    }

                    // добавляем в очередь задание на вывод сообщения о некорректном типе медиафайла

                    if (TextBoxLog.Text.Contains("No streams to output were found"))
                    {
                        MessageBox.Show("Mediafile has no streams or incorrect.");
                    }

                    // добавляем в очередь задание на вывод сообщения о успешном завершении процесса

                    if (TextBoxLog.Text.Contains("Progress: 100%"))
                    {
                        MessageBox.Show("Spliting comlete.");
                    }
                });

        }

        /// <summary> установка в очередь обнвовления лога </summary>
        public void UpdateLogTextbox(object sendingProcess, DataReceivedEventArgs args)
        {
            _taskManager.Add(null,
                delegate
                {
                    MainWindow._wm.TextBoxLog.Text += args.Data + "\n";
                });
        }
        
        /// <summary> проверка существования файла </summary>
        private bool CheckingFileExisting(string path, string message = null)
        {
            if (File.Exists(path)) return false;
            
            if (message != null) MessageBox.Show(message);

            return true;
        }

        /// <summary> нахождение несуществующего имени файла </summary>
        private static string GeneratingFileName(string path)
        {
            var i = 1;
            string newpath;

            //перебираем инкремент в названию файла пока не найдётся несуществующий
            do
            {
                newpath = Path.GetDirectoryName(path) + "\\" +
                          Path.GetFileNameWithoutExtension(path) + i +
                          ".mkv";
                i++;
            } while (File.Exists(newpath));
            
            return newpath;
        }

        /// <summary> обработчик метода вызванного из списка заданий </summary>
        public static void InMainDispatch(Action dlg)
        {
            if (Thread.CurrentThread.Name == "Main Thread")
            {
                dlg();
            }
            else
            {
                _wm.Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(delegate { dlg(); }), "?");
            }
        }
        
        /// <summary> обработчик выхода из программы </summary>
        private void OnAppExit(object sender, EventArgs e)
        {
            //если процесс существует и запущен, закрываем его при выходе из программы
            if (consoleinterface._myProc != null && !consoleinterface._myProc.HasExited) consoleinterface._myProc.Kill();
        }
        
        /// <summary> кнопка добавления экземпляра интерфейса содержащего информацию о временном промежутке </summary>
        private void AddTimeRangeButton_Click(object sender, RoutedEventArgs e)
        {
            //создаём новый объект содержащий интерфейс работы с временным промежутком
            var newNumber = 0;

            var templist = ListOfRanges.Select(range => range.Number).ToList();

            templist.Sort();

            if (templist.Count > 0 && templist.Last() + 1 > templist.Count)
            {
                for (var i = 0; i < templist.Count; i++)
                {
                    if (i >= templist[i]) continue;
                    newNumber = i;
                    break;
                }
            }
            else
            {
                newNumber = ListOfRanges.Count;
            }

            var timeRangeControl = new RangeDispalyControl(
                "00:00:00.000",
                "00:00:00.000",
                (byte) (newNumber),
                (bool) CheckBoxAdditable.IsChecked);


            // и добавляем его в список таковых объектов
            ListOfRanges.Add(timeRangeControl);

            RefreshRanges();

        }

        /// <summary> перестроение списка промежутков времени </summary>
        public void RefreshRanges()
        {
            //очищаем панель с временными промежутками
            StackPanelRanges.Children.Clear();

            //добавляем все промежутки времени в панель по очереди
            foreach (var range in ListOfRanges)
            {
                StackPanelRanges.Children.Add(range);
            }
        }
        
        /// <summary> Ограничение высоты приложения  </summary>
        private void MkvSplitGui_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(RangesScrollViewer.ActualHeight > (Screen.PrimaryScreen.WorkingArea.Height - 201)) MkvSplitGui.Top=20;
        }
        
        
        

        


      

        

        

    
    
    }
}
