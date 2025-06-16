using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Mail2Ticket
{
    public class TicketSearch
    {
        public partial class TicketSuggestion
        {
            public string tn { get; set; }
            public string title { get; set; }
            public string name { get; set; }
        }
        public TicketSearch()
        {
            // Hier können Sie die Logik für die Ticket-Suche implementieren
            // Zum Beispiel: Verbindung zu einer Datenbank herstellen, um Tickets zu suchen
            // oder eine API aufzurufen, um Tickets abzurufen.
        }
        // Beispielmethode zum Suchen von Tickets
        public void SearchTickets(string searchTerm)
        {
            // Implementieren Sie hier die Logik zur Suche von Tickets basierend auf dem Suchbegriff
            // Dies könnte eine Datenbankabfrage oder eine API-Anfrage sein.
        }


        public static async Task Main(string[] args)
        {
            string query = "test";
            string email = "jojo@ulewu.de";
            string url = $"http://localhost:8080/api/tickets/suggestions?q={Uri.EscapeDataString(query)}&mail={Uri.EscapeDataString(email)}";

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // throws if not 2xx

                string json = await response.Content.ReadAsStringAsync();
                List<TicketSuggestion> suggestions = JsonSerializer.Deserialize<List<TicketSuggestion>>(json);

                // Print results
                foreach (var suggestion in suggestions)
                {
                    Console.WriteLine($"Ticket: {suggestion.tn}, Title: {suggestion.title}, Name: {suggestion.name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}