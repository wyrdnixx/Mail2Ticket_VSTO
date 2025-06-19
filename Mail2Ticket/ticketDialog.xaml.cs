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
            _ticketSearch = new TicketSearch();

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
            tbStatusText.Text += Environment.NewLine + statusText;
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
