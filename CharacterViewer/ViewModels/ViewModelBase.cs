using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CharacterViewer.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
