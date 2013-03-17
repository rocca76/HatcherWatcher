using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HatcherWatcher
{
    public class MyViewModel
    {
        private BindingList<String> _logList = new BindingList<String>();

        public BindingList<String> Items
        {
            get { return _logList; }
        }
    }
}
