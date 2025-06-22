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
using System.Xml.Linq;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
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
        public void SetMailKontext(Outlook.MailItem mailItem)
        {

            _mailItem = mailItem;
            tbEmailSubject.Text = _mailItem.Subject.ToString();
            tbSearchString.Text = _mailItem.SenderEmailAddress;
            _ticketSearch = new TicketSearch();
            loadDestinationFolder();



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
                copy.Subject = "[MCB#" + tbTicketNumber.Text + "] " + tbEmailSubject.Text;

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
            tbStatusText.Text += Environment.NewLine + statusText;
        }

        // auto search on initialization
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mailItem != null && _ticketSearch != null)
            {
                // You can customize this initial search string
                _ticketSearch.SearchTickets(tbSearchString.Text, _mailItem.SenderEmailAddress, this);
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
                _ticketSearch.SearchTickets(tbSearchString.Text,_mailItem.SenderEmailAddress, this);
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
        }
    }
}
