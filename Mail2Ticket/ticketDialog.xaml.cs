using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Mail2Ticket
{
    /// <summary>
    /// Interaktionslogik für TicketDialog.xaml
    /// </summary>
    public partial class TicketDialog : UserControl
    {

        private Outlook.MailItem _mailItem;
        private TicketSearch _ticketSearch;

        public TicketDialog()
        {
            InitializeComponent();
        }

        // Übergibt das MailItem-Objekt und setzt den Button-Text
        public void SetMailKontext(Outlook.MailItem mailItem)
        {

            _mailItem = mailItem;
            tbEmailSubject.Text = _mailItem.Subject.ToString();
            tbSearchString.Text = _mailItem.SenderEmailAddress;
            _ticketSearch = new TicketSearch();


        }

        internal void UpdateTicketSearchResults( List<TicketSearch.TicketSuggestion> suggestions)
        {
            SuggestionsDataGrid.Items.Clear();
            
            foreach (var suggestion in suggestions)
            {
             //   SuggestionsDataGrid.Items.Add(suggestion.tn + " - " + suggestion.title + " (" + suggestion.name + ")");
            }
            SuggestionsDataGrid.ItemsSource = suggestions;

        }

        private void SuggestionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SuggestionsDataGrid.SelectedItem is TicketSearch.TicketSuggestion selected)
            {
                
                //MessageBox.Show("Ausgewähltes Ticket: " + selected.tn);
                tbTicketNumber.Text = selected.tn; // Setzt die Ticketnummer in die TextBox
            }
            
        }

        private void btnMail2Ticket_Click(object sender, RoutedEventArgs e)
        {
            if (_mailItem != null)
            {
                try
                {
                    // Kopie der Mail erstellen
                    Outlook.MailItem copy = (Outlook.MailItem)_mailItem.Copy();

                    //  Ticketnummer aus TextBox verwenden, falls vorhanden
                    copy.Subject = "[MCB#" + tbTicketNumber.Text + "] " + tbEmailSubject.Text; // Optional: Betreff anpassen

                    // In den Entwürfe-Ordner verschieben
                    Outlook.MAPIFolder drafts = _mailItem.Application.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderDrafts);
                    copy.Move(drafts);



                    //MessageBox.Show("Kopie der E-Mail wurde im Ordner 'Entwürfe' erstellt.");
                    // Optional: Dialog schließen oder weitere Aktionen durchführen 
                    Window.GetWindow(this)?.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Kopieren: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Kein MailItem übergeben.");
            }
        }

        public void setStatusText(string statusText)
        {
           // tbStatusText.Text += Environment.NewLine + statusText;
        }

        // auto search on initialization
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mailItem != null && _ticketSearch != null)
            {
                // You can customize this initial search string
                _ticketSearch.SearchTickets(tbSearchString.Text, this);
            }
        }
        private void tbSearchString_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && tbSearchString.Text.Length > 2)
            {

                //MessageBox.Show(    "Suche nach: " + tbSearchString.Text);
                // Hier können Sie die Logik zur Suche von Tickets basierend auf dem Suchbegriff implementieren
                // Zum Beispiel: Verbindung zu einer Datenbank herstellen, um Tickets zu suchen
                // oder eine API aufzurufen, um Tickets abzurufen.
                _ticketSearch.SearchTickets(tbSearchString.Text, this);
            }
        }
    }
}
