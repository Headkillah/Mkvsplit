using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace mkvsplit
{
    class consoleinterface
    {
        /// <summary> процесс запускающий консольное приложение  </summary>
        public static Process _myProc;

        

        



        /// <summary> запуск приложения | путь к файлу, [аргументы], [ожидать ли окончания] </summary>
        public static void Start(string filename, string arguments = "", bool waitforexit = true)
        {
            //проверяем существует ли приложение и если существует, то подписано ли оно как mkvmerge
            if (File.Exists(filename) && FileVersionInfo.GetVersionInfo(filename).InternalName == "mkvmerge")
            {
                //создаём класс запускаемого процесса
                _myProc = new Process
                {
                    StartInfo =
                    {
                        //сообщаем классу процесса путь к исполняемому приложению
                        FileName = "\"" + filename + "\"",
                        Arguments = arguments,
                        //не показывать окно приложения 
                        CreateNoWindow = true,
                        //перенапрвляем текст
                        RedirectStandardOutput = true,
                        //запуск самого процесса без выбора способа открытия ОС
                        UseShellExecute = false
                    },
                    //включает событие "процесс вышел"
                    EnableRaisingEvents = true
                };


                //добавляем обработчик окончания выполнения процесса myProc
                _myProc.Exited += MainWindow._wm.OnProcessExited;

                //перенаправляем вывод консольного приложения в в окно лога
                _myProc.OutputDataReceived += MainWindow._wm.UpdateLogTextbox;


                try
                {
                    _myProc.Start();
                }

                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                // начaло асинхронного считывания
                _myProc.BeginOutputReadLine();

                //при необходимости ждём пока не завершится процесс
                if (waitforexit) _myProc.WaitForExit();

            }

            else
            {
                MessageBox.Show("\nMKVMerge is not found.\n");
                MainWindow._wm.TextBoxLog.Text += "\nMKVMerge is not found.\n";

            }
        }
    }
}
