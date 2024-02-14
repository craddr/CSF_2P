using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient
{
    public class MyTraceListener : TraceListener, INotifyPropertyChanged
    {
        private string msg;
        private string category;

        public MyTraceListener()
        {
            this.msg = "";
            this.category = "";
        }

        public string Trace
        {
            get { return this.msg; }
        }

        public string Category
        {
            get { return this.category; }
        }

        public override void Write(string message)
        {
            this.msg = message;
            this.OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        public override void WriteLine(string message)
        {
            this.msg = message;
            this.category = "";
            this.OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        public override void WriteLine(string message, string category)
        {
            this.msg = message;
            this.category = category;
            this.OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
