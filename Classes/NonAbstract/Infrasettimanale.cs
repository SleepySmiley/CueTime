using System.Collections.ObjectModel;
using InTempo.Classes.Abstract;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.NonAbstract
{
    public class Infrasettimanale : RiunioniGenerali
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new();

        public async Task LoadAsync()
        {
            ApplyLoadedParts(await WebPartsLoader.CaricaInfrasettimanaleAsync());
        }

        public void LoadFromCache()
        {
            ApplyLoadedParts(WebPartsLoader.CaricaInfrasettimanaleDaCache());
        }

        public Parte this[int i]
        {
            get => Parti[i];
            set => Parti[i] = value;
        }

        private void ApplyLoadedParts(ObservableCollection<Parte> loaded)
        {
            Parti.Clear();

            foreach (Parte parte in loaded)
            {
                Parti.Add(parte);
            }
        }
    }
}
