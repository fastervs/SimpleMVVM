using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Linq;
using SimpleMVVM.Models;

namespace SimpleMVVM.VM
{
    class ViewModel: INotifyPropertyChanged
    {
        
        private readonly object _urlsLock= new object();

        public ObservableCollection<URL> URLs { get; set; }//Коллекция загруженных из XML файла ссылок, и кол-ву тэгов по каждой


        public ViewModel()
        {

            URLs = new ObservableCollection<URL>();
            BindingOperations.EnableCollectionSynchronization(URLs, _urlsLock);
        }

        
       public CancellationTokenSource cts;


        private Relay_Command _start;// Запуск анализа всех ссылок на количество тэгов
        private Relay_Command _addURL;//Команда добавления данных из XML
        private Relay_Command _stop;// Стоп 

        private int _max = 100;//Максимальное значение для progressbar
        private int _min = 0;//Минимальное значение для progressbar
        private int _progress = 0;//Текущее значение для progressbar
        private URL _maxtags = new URL();//Ссылка с максимальным количеством элементов

        public URL Maxtags
        {
            get { return _maxtags; }
            set
            {
                _maxtags = value;
                OnPropertyChanged("Maxtags");
            }
        }


        public int Maximum {
            get { return _max; }
            set
            {
                _max = value;
                OnPropertyChanged("Maximum");
            }
        }

        public int Minimum {
            get { return _min; }
            set
            {
                _min = value;
                OnPropertyChanged("Min");
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }


        //Запуск асинхронно анализа всех ссылок на количество тэгов
        //Количество потоков равно количеству ссылок, так как каждый поток использует крайне малое количество процессорной мощности
        //большая часть времени тратится на ожидание ответа от сервера
        public  async Task  Run_it()
        {
            cts=new CancellationTokenSource();
            await Task.WhenAll(URLs.Select(e => e.Process(cts).ContinueWith(t => Progress++)).ToArray());//Progressbar отображает кол-во завершенных потоков (обработанных ссылок)
            Maxtags = URLs.OrderByDescending(e => e.Count).Take(1).ToArray()[0];//Находим ссылку с максимальным количеством тэгов из ранее проанализированных
        }



        public Relay_Command T_start
        {
            get
            {
                return _start ??
                  (_start = new Relay_Command( async obj => await  Run_it()));
            }
        }

        public Relay_Command T_stop
        {
            get
            {
                //Отмена обработки ссылок выдаст null для ссылки в случае отмены
                return _stop ??
                  (_stop = new Relay_Command( obj => {
                      try
                      {
                          cts.Cancel();
                      }
                      catch
                      {

                      }
                  }));
            }
        }

        public Relay_Command addURL
        {
            get
            {
                //Реализация добавления ссылок из XML файла
                return _addURL ??
                  (_addURL = new Relay_Command(obj =>
                  {
                      var xml = XDocument.Load(@"XML1.xml");
                      int count = 0;
                      foreach (var item in  xml.Descendants("URL").Select(x => new URL { URI=(string)x.Value, Count=0}))
                      {
                          URLs.Insert(0,item);
                          count++;
                      }
                      Maximum = count;
                  }));
            }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
