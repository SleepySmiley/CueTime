using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace InTempo
{
    public partial class MainWindow : Window
    {
        public Adunanza DatiAdunanza { get; set; } = new Adunanza();
        public TimerLogics LogicTimer { get; set; }
        private bool _isPaused = true;

        public MainWindow()
        {
            InitializeComponent();
            LogicTimer = new TimerLogics(DatiAdunanza);
            DataContext = this;
        }

        private void BtnAvanti_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Avanti();
        }

        private void BtnPausaRiprendi_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                LogicTimer.StartTimer();
                _isPaused = false;
            }
            else
            {
                LogicTimer.StopTimer();
                _isPaused = true;
            }
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Indietro();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DatiAdunanza.SelectedAdunanza();
        }

        private void btnOptions_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // blocca la selezione della riga da parte del DataGrid
            e.Handled = true;

            if (sender is FrameworkElement fe && fe.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = fe;
                menu.Placement = PlacementMode.Bottom;
                menu.IsOpen = true;
            }
        }

        private void MenuItemReset_Click(object sender, RoutedEventArgs e)
        {
            Parte parteSelezionata = TrovaParte(sender);
            if (parteSelezionata != null)
            {
                LogicTimer.ResetTimerPreciso(parteSelezionata);
            }
        }

        private Parte TrovaParte(object sender)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var button = (Button)contextMenu.PlacementTarget;
            Parte parteSelezionata = (Parte)button.DataContext;
            return parteSelezionata;
        }

        private void MenuItemAggiungi_Click(object sender, RoutedEventArgs e)
        {
            // Metto in pausa per evitare bug sul tempo residuo durante il dialog
            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            Parte parteSelezionata = TrovaParte(sender);
            int indice = DatiAdunanza.Parti.IndexOf(parteSelezionata);

            Classes.View.ModificaParte finestra = new Classes.View.ModificaParte();

            if (finestra.ShowDialog() == true)
            {
                // Dopo aver creato una parte aggiungo al temporesiduo il tempo della nuova parte
                // CORREZIONE: Se aggiungo una parte, consumo tempo disponibile -> Sottraggo
                DatiAdunanza.TempoResiduo -= finestra.ParteCopia.TempoParte;

                DatiAdunanza.Parti.Insert(indice + 1, finestra.ParteCopia);
            }

            // Riprendo il timer se era attivo
            if (wasRunning) LogicTimer.StartTimer();
        }

        private void MenuItemEllimina_Click(object sender, RoutedEventArgs e)
        {
            Parte parteSelezionata = TrovaParte(sender);

            // Prima di eliminare la parte sottraggo al temporesiduo il tempo della parte eliminata
            // CORREZIONE: Se elimino una parte, libero tempo -> Aggiungo
            DatiAdunanza.TempoResiduo += parteSelezionata.TempoParte;

            DatiAdunanza.Parti.Remove(parteSelezionata);
        }

        private void MenuItemModifica_Click(object sender, RoutedEventArgs e)
        {
            // Metto in pausa per evitare bug sul tempo residuo durante il dialog
            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            Parte parteSelezionata = TrovaParte(sender);

            if (parteSelezionata == null) return;

            Classes.View.ModificaParte finestra = new Classes.View.ModificaParte(parteSelezionata);

            if (finestra.ShowDialog() == true)
            {
                int index = DatiAdunanza.Parti.IndexOf(parteSelezionata);

                if (index != -1)
                {
                    // Calcolo la differenza tra il nuovo tempo totale e quello vecchio
                    TimeSpan differenzaTempo = finestra.ParteCopia.TempoParte - parteSelezionata.TempoParte;

                    // Aggiorno il tempo residuo in base alla differenza
                    // CORREZIONE: Se aumento la durata (diff > 0), il residuo diminuisce -> Sottraggo
                    DatiAdunanza.TempoResiduo -= differenzaTempo;

                    // Aggiorno le proprietà mantenendo il puntatore fisso
                    DatiAdunanza.Parti[index].NumeroParte = finestra.ParteCopia.NumeroParte;
                    DatiAdunanza.Parti[index].NomeParte = finestra.ParteCopia.NomeParte;
                    DatiAdunanza.Parti[index].TempoParte = finestra.ParteCopia.TempoParte;
                    DatiAdunanza.Parti[index].TipoParte = finestra.ParteCopia.TipoParte;
                    DatiAdunanza.Parti[index].ColoreParte = finestra.ParteCopia.ColoreParte;

                    // In caso la parte modificata sia quella in corso di esecuzione, aggiorno il timer preciso
                    if (DatiAdunanza.Parti[index] == DatiAdunanza.Current)
                    {
                        DatiAdunanza.Parti[index].TempoScorrevole += differenzaTempo;
                    }
                    else
                    {
                        // Se non è in esecuzione, aggiorno il tempo scorrevole al nuovo tempo totale
                        DatiAdunanza.Parti[index].TempoScorrevole = finestra.ParteCopia.TempoParte;
                    }
                }
            }

            // Riprendo il timer se era attivo
            if (wasRunning) LogicTimer.StartTimer();
        }
    }
}