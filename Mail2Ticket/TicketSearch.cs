using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Mail2Ticket
{
    internal class TicketSearch
    {

        public partial class TicketSuggestion
        {
            public string type { get; set; }
            public string kunde { get; set; }
            public string tn { get; set; }
            public string title { get; set; }
            
        }
        public TicketSearch()
        {
            // Hier können Sie die Logik für die Ticket-Suche implementieren
            // Zum Beispiel: Verbindung zu einer Datenbank herstellen, um Tickets zu suchen
            // oder eine API aufzurufen, um Tickets abzurufen.
        }

        public async void getClientVersion(string _SearchServer, TicketDialog dialog)
        {
            string url = $"http://{_SearchServer}/api/getClientVersion";

            HttpClient client = new HttpClient();
            try { 
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // throws if not 2xx
                string json = await response.Content.ReadAsStringAsync();
                var versionInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                //MessageBox.Show($"Client Version: {versionInfo["version"]}");
                if (versionInfo != null && versionInfo.ContainsKey("clientVersion"))
                {
                    dialog.setStatusText($"Benötigte Client Version: {versionInfo["clientVersion"]}");
                    //Console.WriteLine($"Client Version: {versionInfo["version"]}");
                    dialog.checkClientVersion(versionInfo["clientVersion"]);
                }
                else
                {
                    dialog.setStatusText("Keine benötigte Client-Version gefunden.");
                    dialog.checkClientVersion(null);
                    //Console.WriteLine("Keine Client-Version gefunden.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                dialog.setStatusText($"Fehler bei der Suche: {ex.Message}");
                dialog.checkClientVersion(null);
            }
}


        // Suche Tickets
        public async void  SearchTickets(string _SearchServer, string _searchTerm, string _SenderEmailAddress, TicketDialog dialog)
        {
            // Implementieren Sie hier die Logik zur Suche von Tickets basierend auf dem Suchbegriff
            // Dies könnte eine Datenbankabfrage oder eine API-Anfrage sein.
            

            dialog.setStatusText("Suche...");

            string query = _searchTerm;
            string email = _SenderEmailAddress;
            //string url = $"http://localhost:8080/api/tickets/suggestions?q={Uri.EscapeDataString(query)}&mail={Uri.EscapeDataString(email)}";
            string url = $"http://{_SearchServer}/api/tickets/suggestions?q={Uri.EscapeDataString(query)}&mail={Uri.EscapeDataString(email)}";

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // throws if not 2xx

                string json = await response.Content.ReadAsStringAsync();
                List<TicketSuggestion> suggestions = JsonSerializer.Deserialize<List<TicketSuggestion>>(json);

                // Print results if found
                if (suggestions == null || suggestions.Count == 0)
                {
                    dialog.setStatusText("Keine Tickets gefunden.");
                    //Console.WriteLine("Keine Tickets gefunden.");
                    return;
                } else
                {
                    //foreach (var suggestion in suggestions)
                    //{
                    //    Console.WriteLine($"Ticket: {suggestion.tn}, Title: {suggestion.title}, Name: {suggestion.type}");

                    //}
                    dialog.setStatusText("Tickets gefunden: "+ suggestions.Count);
                    dialog.UpdateTicketSearchResults(suggestions);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                dialog.setStatusText($"Fehler bei der Suche: {ex.Message}");
            }
        }

        
       
    }
}
