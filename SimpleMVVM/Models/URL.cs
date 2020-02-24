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
        public string URI
        {
            get { return (string)GetValue(URIProperty); }
            set { SetValue(URIProperty, value); }
        }

        // Using a DependencyProperty as the backing store for URL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty URIProperty =
            DependencyProperty.Register("URI", typeof(string), typeof(URL), new PropertyMetadata(""));

        


        public int? Count
        {
            get { return (int?)GetValue(CountProperty); }
            set { SetValue(CountProperty, value);}
        }

        public static readonly DependencyProperty CountProperty =
                DependencyProperty.Register("Count", typeof(int?), typeof(URL), new PropertyMetadata(0));


public async Task Process(CancellationTokenSource cts)
            => await Task.Run(() =>
        {
            var uri = Application.Current.Dispatcher.Invoke(() => this.URI);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = 2000;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() => Count = null);
                return;
            }

            if (cts.IsCancellationRequested)
            {
                //startpos = v;
                response.Close();
                return;
            }


            using (Stream stream = response.GetResponseStream())
            {
                int count = 0;
              //  Count = 0;
                using (StreamReader reader = new StreamReader(stream))
                {

                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        //if (line.Contains("<a "))
                        //{

                            //count++;
                            count += Regex.Matches(line, "<a\\s").Count;
                            Application.Current.Dispatcher.InvokeAsync(() => Count = count);
                        //}
                        //int res=
                    }
                }
                
            }

            response.Close();
        });
        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        
    }
}
