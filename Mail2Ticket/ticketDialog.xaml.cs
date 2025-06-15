using System;
using System.Windows;
using System.Windows.Controls;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Mail2Ticket
{
    /// <summary>
    /// Interaktionslogik für ticketDialog.xaml
    /// </summary>
    public partial class ticketDialog : UserControl
    {
        private Outlook.MailItem _mailItem;

        public ticketDialog()
        {
            InitializeComponent();

            
        }

        // Übergibt das MailItem-Objekt und setzt den Button-Text
        public void SetMailKontext( Outlook.MailItem mailItem)
        {
            
            _mailItem = mailItem;
            tbEmailSubject.Text = _mailItem.Subject.ToString();
        }

       

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Konfigurationsbutton wurde geklickt!");
        }

        private void btnSend2Ticket_Click(object sender, RoutedEventArgs e)
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
    }
}
