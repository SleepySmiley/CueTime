using System.Collections.ObjectModel;
using System;
using CueTime.Classes.Abstract;
using CueTime.Classes.Utilities;

namespace CueTime.Classes.NonAbstract
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

