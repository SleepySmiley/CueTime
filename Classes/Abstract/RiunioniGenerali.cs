using System.Collections.ObjectModel;
using InTempo.Classes.NonAbstract;

namespace InTempo.Classes.Abstract
{
    public abstract class RiunioniGenerali
    {
        public abstract ObservableCollection<Parte> Parti { get; set; }
    }
}
