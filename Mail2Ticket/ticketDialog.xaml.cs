using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Xml.Linq;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
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
        private Outlook.MAPIFolder _selectedFolder;
        


        public TicketDialog()
        {
            InitializeComponent();
        }

        // Übergibt das MailItem-Objekt und setzt den Button-Text
        public void StartMail2Ticket(Outlook.MailItem mailItem)
        {

            _mailItem = mailItem;
            tbEmailSubject.Text = _mailItem.Subject.ToString();
            //tbSearchString.Text = _mailItem.SenderEmailAddress;
            //tbSearchString.Text = _mailItem.Sender.Address; // Setzt die E-Mail-Adresse des Absenders in die TextBox
            tbSearchString.Text = GetSmtpAddress(_mailItem);// Setzt die E-Mail-Adresse des Absenders in die TextBox 

            _ticketSearch = new TicketSearch();
            loadDestinationFolder();

            //Properties.Settings.Default.SearchServer = "localhost:8080";
            tbSearchServer.Text = Properties.Settings.Default.SearchServer;
            _ticketSearch.getClientVersion(Properties.Settings.Default.SearchServer, this);
        }


        public string GetSmtpAddress(Outlook.MailItem mail) // get the real email address of the sender if exchange user - without it will return the x500 address
        {
            Outlook.AddressEntry sender = mail.Sender;

            if (sender != null)
            {
                if (sender.AddressEntryUserType == Outlook.OlAddressEntryUserType.olExchangeUserAddressEntry ||
                    sender.AddressEntryUserType == Outlook.OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
                {
                    // Sender is an Exchange user
                    Outlook.ExchangeUser exchUser = sender.GetExchangeUser();
                    if (exchUser != null)
                    {
                        return exchUser.PrimarySmtpAddress;
                    }
                }
                else
                {
                    // Sender is probably SMTP (external)
                    return sender.Address;
                }
            }
            return null;
        }
        private void loadDestinationFolder()
        {
            // Show saved folder path, if available
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastUsedFolderEntryID))
            {
                try
                {
                    Outlook.NameSpace session = new Outlook.Application().Session;
                    var folder = session.GetFolderFromID(
                        Properties.Settings.Default.LastUsedFolderEntryID,
                        Properties.Settings.Default.LastUsedFolderStoreID);

                    if (folder != null)
                    {
                        //setStatusText("Zielordner: " + folder.FolderPath);
                        lblDestinationFolder.Content = "Zielordner: " + folder.FolderPath;
                    }
                }
                catch
                {
                    //setStatusText("Zielordner nicht festgelegt...");
                    lblDestinationFolder.Content = "Zielordner fehler..." ;
                }
            } else
            {
                lblDestinationFolder.Content = "Zielordner nicht festgelegt...";
            }
        }

        internal void UpdateTicketSearchResults( List<TicketSearch.TicketSuggestion> suggestions)
        {

            
            SuggestionsDataGrid.ItemsSource = null; // Reset necessary to refresh
            SuggestionsDataGrid.ItemsSource = suggestions;

            //SuggestionsDataGrid.Items.Clear();

            //foreach (var suggestion in suggestions)
            //{
            //SuggestionsDataGrid.Items.Add(suggestion.tn + " - " + suggestion.title + " (" + suggestion.name + ")");
            //}

            //SuggestionsDataGrid.ItemsSource = suggestions;

            // ToDO: Bug beim zweiten Suchen:
            // Der Vorgang ist während der Verwendung von "ItemsSource" ungültig.Verwenden Sie stattdessen "ItemsControl.ItemsSource", um auf Elemente zuzugreifen und diese zu ändern.

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


            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LastUsedFolderEntryID) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.LastUsedFolderStoreID))
            {
                MessageBox.Show("Bitte zuerst Zielordner festlegen...");
                return;
            }

            if (_mailItem == null)
            {
                MessageBox.Show("Kein MailItem übergeben.");
                return;
            }

            try
            {
                Outlook.MailItem copy = (Outlook.MailItem)_mailItem.Copy();

                if (cbNewTicket.IsChecked == true)
                {
                    // Wenn "Neues Ticket" ausgewählt ist, Ticketnummer leeren
                    tbTicketNumber.Text = string.Empty;
                    
                } else 
                {
                    if (string.IsNullOrWhiteSpace(tbTicketNumber.Text))
                    {
                        MessageBox.Show("Bitte geben Sie eine Ticketnummer ein oder wählen Sie 'Neues Ticket' aus.");
                        return;
                    }
                    // Wenn "Neues Ticket" nicht ausgewählt ist, Ticketnummer setzen
                    copy.Subject = "[MCB#" + tbTicketNumber.Text + "] " + tbEmailSubject.Text;
                }
                    

                Outlook.MAPIFolder targetFolder = GetFolderFromEntryID(
                    Properties.Settings.Default.LastUsedFolderEntryID,
                    Properties.Settings.Default.LastUsedFolderStoreID);

                if (targetFolder != null)
                {
                    copy.Move(targetFolder);
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("Zielordner konnte nicht gefunden werden.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Kopieren: " + ex.Message);
            }
        }


        private Outlook.MAPIFolder GetFolderFromEntryID(string entryID, string storeID)
        {
            try
            {
                Outlook.NameSpace session = _mailItem.Application.Session;
                return session.GetFolderFromID(entryID, storeID);
            }
            catch
            {
                return null;
            }
        }

        public void setStatusText(string statusText)
        {
            if (tbStatusText.Dispatcher.CheckAccess())
            {
                tbStatusText.Text += Environment.NewLine + statusText;
                tbStatusText.ScrollToEnd();
            }
            else
            {
                tbStatusText.Dispatcher.Invoke(() =>
                {
                    tbStatusText.Text += Environment.NewLine + statusText;
                    tbStatusText.ScrollToEnd();
                });
            }
        }

        // auto search on initialization
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mailItem != null && _ticketSearch != null)
            {
                // You can customize this initial search string
                _ticketSearch.SearchTickets(Properties.Settings.Default.SearchServer, tbSearchString.Text, _mailItem.SenderEmailAddress, this);
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
                _ticketSearch.SearchTickets(Properties.Settings.Default.SearchServer, tbSearchString.Text,_mailItem.SenderEmailAddress, this);
            }
        }

                private void tbSearchServer_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && tbSearchString.Text.Length > 2)
            {
                MessageBoxResult result = MessageBox.Show(
      "Do you want to save the settings?",    // Message text
      "Save Settings",                        // Title
      MessageBoxButton.YesNo,                 // Buttons
      MessageBoxImage.Question                // Icon
  );

                if (result == MessageBoxResult.Yes)
                {
                    // User chose Yes
                    // Save settings here
                    Properties.Settings.Default.SearchServer = tbSearchServer.Text;
                    Properties.Settings.Default.Save();

                }
                else
                {
                    // User chose No
                    tbSearchServer.Text = Properties.Settings.Default.SearchServer; // Revert to last saved value
                }
            }
        }
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Outlook.Application outlookApp = new Outlook.Application();
                Outlook.NameSpace outlookNamespace = outlookApp.GetNamespace("MAPI");

                Outlook.MAPIFolder folder = outlookNamespace.PickFolder();

                if (folder != null)
                {
                    _selectedFolder = folder;
                    //MessageBox.Show("Ordner ausgewählt: " + folder.Name + " (" + folder.FolderPath + ")", "Konfiguration", MessageBoxButton.OK, MessageBoxImage.Information);

                    //Speichern in Konfigdatei
                    //Properties.Settings.Default.LastUsedFolder = folder.FolderPath;
                    

                    //Properties.Settings.Default.LastUsedFolder = _selectedFolder.FolderPath;
                    //Properties.Settings.Default.Save();

                    Properties.Settings.Default.LastUsedFolderEntryID = _selectedFolder.EntryID;
                    Properties.Settings.Default.LastUsedFolderStoreID = _selectedFolder.StoreID;
                    Properties.Settings.Default.Save();



                    setStatusText($"Ordner ausgewählt: {folder.Name} ({folder.FolderPath})");
                    loadDestinationFolder(); // Update the displayed folder path
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Öffnen des Dialogs: " + ex.Message);
            }

            // Bring addin window to front
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Topmost = true;     // Make topmost temporarily
                parentWindow.Activate();         // Bring to foreground
                parentWindow.Topmost = false;    // Revert so it behaves normally again
            }
        }

        public void checkClientVersion(string _versionFromServer)
        {
            //MessageBox.Show(_versionFromServer, "Client Version", MessageBoxButton.OK, MessageBoxImage.Information);

            if (_versionFromServer == null)
            {
                setStatusText("Keine Client-Version gefunden. Bitte überprüfen Sie die Verbindung zum Server.");
                return;
            }
            else if (_versionFromServer == Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                setStatusText("Client-Version ist aktuell: " + _versionFromServer);
            }
            else
            {
                
                MessageBox.Show(@"Client-Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ist veraltet. Bitte aktualisieren Sie den Client auf Version: " + _versionFromServer, "Client Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                Window.GetWindow(this)?.Close();

            }
        }

        private void cbNewTicket_Checked(object sender, RoutedEventArgs e)
        {
            if (cbNewTicket.IsChecked == true)
            {
                // Wenn das Kontrollkästchen aktiviert ist, Ticketnummer leeren
                tbTicketNumber.Text = string.Empty;
                tbTicketNumber.IsEnabled = false; // Deaktivieren der TextBox
            }
            else
            {
                // Wenn das Kontrollkästchen deaktiviert ist, Ticketnummer aktivieren
                tbTicketNumber.IsEnabled = true;
            }
        }
    }
}
