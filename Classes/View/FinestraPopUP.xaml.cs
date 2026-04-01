using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InTempo.Classes.View
{
    public partial class FinestraPopUP : Window
    {
        public string Titolo { get; set; }
        public string TestoMessaggio { get; set; } = string.Empty;
        public string TestoInput { get; set; } = string.Empty;

        public ModalitaPopup TipologiaFinestra { get; set; }

        public int? NumeroInserito { get; private set; }

        private readonly FinestraTimer? _finestratimer;
        private readonly bool _richiedeNumeroInteroPositivo;

        public FinestraPopUP(string titoloPassato, string testoPassato, ConfigurazionePulsantiPopup configurazionePulsanti)
        {
            InitializeComponent();

            switch (configurazionePulsanti)
            {
                case ConfigurazionePulsantiPopup.Nessuno:
                    BtnPredefinito1.Visibility = Visibility.Collapsed;
                    BtnPredefinito2.Visibility = Visibility.Collapsed;
                    break;
                case ConfigurazionePulsantiPopup.Ok:
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    BtnPredefinito1.Visibility = Visibility.Collapsed;
                    break;
                case ConfigurazionePulsantiPopup.ConfermaAnnulla:
                    BtnPredefinito1.Visibility = Visibility.Visible;
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    break;
            }

            Titolo = titoloPassato;
            TestoMessaggio = testoPassato;
            DataContext = this;
            txbTesto.Visibility = Visibility.Visible;
            txtTesto.IsReadOnly = true;
            TipologiaFinestra = ModalitaPopup.Messaggio;
        }

        public FinestraPopUP(string titoloPassato, string testoPassato, string testoBtn1, string testoBtn2)
        {
            InitializeComponent();
            Titolo = titoloPassato;
            TestoMessaggio = testoPassato;
            txbTesto.Visibility = Visibility.Visible;
            BtnPredefinito1.Content = testoBtn1;
            BtnPredefinito2.Content = testoBtn2;
            DataContext = this;
            txtTesto.IsReadOnly = true;
            TipologiaFinestra = ModalitaPopup.Conferma;
        }

        public FinestraPopUP(string titoloPassato, string testoBtn1, string testoBtn2, FinestraTimer finestraDaPassare)
        {
            InitializeComponent();
            Titolo = titoloPassato;
            txtTesto.Visibility = Visibility.Visible;
            TestoInput = string.Empty;
            DataContext = this;
            BtnPredefinito1.Content = testoBtn1;
            BtnPredefinito2.Content = testoBtn2;
            TipologiaFinestra = ModalitaPopup.MessaggioSchermo;
            _finestratimer = finestraDaPassare;
        }

        public FinestraPopUP(string titoloPassato, string testoPassato, string testoBtn1, string testoBtn2, bool richiedeNumeroInteroPositivo)
        {
            InitializeComponent();
            Titolo = titoloPassato;
            TestoMessaggio = testoPassato;
            TestoInput = string.Empty;
            DataContext = this;
            BtnPredefinito1.Content = testoBtn1;
            BtnPredefinito2.Content = testoBtn2;
            txbTesto.Visibility = Visibility.Visible;
            txtTesto.Visibility = Visibility.Visible;
            txtTesto.IsReadOnly = false;
            TipologiaFinestra = ModalitaPopup.Input;
            _richiedeNumeroInteroPositivo = richiedeNumeroInteroPositivo;
            Loaded += (_, _) => txtTesto.Focus();
        }

        private void BtnPredefinito2_Click(object sender, RoutedEventArgs e)
        {
            if (TipologiaFinestra == ModalitaPopup.Messaggio || TipologiaFinestra == ModalitaPopup.Conferma)
            {
                DialogResult = true;
                Close();
                return;
            }

            if (TipologiaFinestra == ModalitaPopup.MessaggioSchermo)
            {
                ApplicaMessaggio(VistaPresentazione.Mista);
                return;
            }

            if (!TryConfermaInput())
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnPredefinito1_Click(object sender, RoutedEventArgs e)
        {
            if (TipologiaFinestra == ModalitaPopup.Messaggio || TipologiaFinestra == ModalitaPopup.Conferma || TipologiaFinestra == ModalitaPopup.Input)
            {
                DialogResult = false;
                Close();
                return;
            }

            ApplicaMessaggio(VistaPresentazione.SoloScritta);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TipologiaFinestra == ModalitaPopup.MessaggioSchermo)
            {
                _finestratimer?.CambiaVista(VistaPresentazione.SoloTimer, string.Empty, Brushes.White);
            }
        }

        private bool TryConfermaInput()
        {
            string input = txtTesto.Text.Trim();

            if (_richiedeNumeroInteroPositivo)
            {
                if (!int.TryParse(input, out int numero) || numero <= 0)
                {
                    MostraErroreInput("Inserisci un numero intero valido maggiore di zero.");
                    return false;
                }

                NumeroInserito = numero;
            }
            else
            {
                TestoInput = input;
            }

            NascondiErroreInput();
            return true;
        }

        private void MostraErroreInput(string messaggio)
        {
            txtErroreInput.Text = messaggio;
            txtErroreInput.Visibility = Visibility.Visible;
        }

        private void NascondiErroreInput()
        {
            txtErroreInput.Text = string.Empty;
            txtErroreInput.Visibility = Visibility.Collapsed;
        }

        private void ApplicaMessaggio(VistaPresentazione tipoVista)
        {
            string input = txtTesto.Text;
            _finestratimer?.CambiaVista(tipoVista, input, Brushes.White);
            txtTesto.Focus();
            txtTesto.SelectAll();
        }

        private void txtTesto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtErroreInput.Visibility == Visibility.Visible)
            {
                NascondiErroreInput();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnChiudiIcona_Click(object sender, RoutedEventArgs e)
        {
            if (TipologiaFinestra == ModalitaPopup.MessaggioSchermo)
            {
                Close();
                return;
            }

            DialogResult = false;
            Close();
        }
    }
}
