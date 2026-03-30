using System.Collections.ObjectModel;
using CueTime.Classes.NonAbstract;

namespace CueTime.Classes.Abstract
{
    public abstract class RiunioniGenerali
    {
        public abstract ObservableCollection<Parte> Parti { get; set; }
    }
}

