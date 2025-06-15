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
        public void SetSenderAndMail(string sender, Outlook.MailItem mailItem)
        {
            TestButton.Content = sender;
            _mailItem = mailItem;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mailItem != null)
            {
                try
                {
                    // Kopie der Mail erstellen
                    Outlook.MailItem copy = (Outlook.MailItem)_mailItem.Copy();
                    // In den Entwürfe-Ordner verschieben
                    Outlook.MAPIFolder drafts = _mailItem.Application.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderDrafts);
                    copy.Move(drafts);
                    MessageBox.Show("Kopie der E-Mail wurde im Ordner 'Entwürfe' erstellt.");
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

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Konfigurationsbutton wurde geklickt!");
        }
    }
}
