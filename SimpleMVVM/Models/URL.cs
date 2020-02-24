using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleMVVM.Models
{
    public class URL: DependencyObject
    {
        public string URI//Cсылка 
        {
            get { return (string)GetValue(URIProperty); }
            set { SetValue(URIProperty, value); }
        }

       
        public static readonly DependencyProperty URIProperty =
            DependencyProperty.Register("URI", typeof(string), typeof(URL), new PropertyMetadata(""));

        


        public int? Count//Количество тэгов по ссылке
        {
            get { return (int?)GetValue(CountProperty); }
            set { SetValue(CountProperty, value);}
        }

        public static readonly DependencyProperty CountProperty =
                DependencyProperty.Register("Count", typeof(int?), typeof(URL), new PropertyMetadata(0));



        //Подсчёт количества тэгов по ссылке
public async Task Process(CancellationTokenSource cts)
            => await Task.Run(() =>
        {
            var uri = Application.Current.Dispatcher.Invoke(() => this.URI);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = 2000;
            HttpWebResponse response = null;

            if (cts.IsCancellationRequested)//Отмена только во время получения ответа от сервера
            {

                Application.Current.Dispatcher.Invoke(() => Count = null);
                return;

            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                
            }
            

            using (Stream stream = response.GetResponseStream())//Поиск тэга с использование регулряного выражения (быстро в реализации) 
            {
                int count = 0;

                using (StreamReader reader = new StreamReader(stream))
                {

                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
 
                            count += Regex.Matches(line, "<a\\s").Count;
                            Application.Current.Dispatcher.InvokeAsync(() => Count = count);

                    }
                }
                
            }

            response.Close();
        });
   
        
    }
}
