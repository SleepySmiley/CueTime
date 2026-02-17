namespace InTempo.Classes
{
    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; }
        public OrarioAdunanza FineSettimana { get; set; }

        public ImpostazioniAdunanze()
        {
            // Valori di default (es. Mercoledì 19:30 e Domenica 10:00)
            Infrasettimanale = new OrarioAdunanza(DayOfWeek.Wednesday, 19, 30);
            FineSettimana = new OrarioAdunanza(DayOfWeek.Sunday, 10, 00);
        }
    }
}