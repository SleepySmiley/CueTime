using System.Collections.ObjectModel;
using System;
using InTempo.Classes.Abstract;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.NonAbstract
{
    public class Finesettimanale : RiunioniGenerali
    {
        public DateTime DataRiferimento { get; set; } = DateTime.Today;

        public override ObservableCollection<Parte> Parti { get; set; } = new();

        public async Task LoadAsync()
        {
            ApplyLoadedParts(await WebPartsLoader.CaricaFineSettimanaAsync(DataRiferimento));
        }

        public void LoadFromCache()
        {
            ApplyLoadedParts(WebPartsLoader.CaricaFineSettimanaDaCache(DataRiferimento));
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
