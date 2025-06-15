using System;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;

namespace Mail2Ticket
{
    [ComVisible(true)]
    public class Mail2TicketRibbon : Office.IRibbonExtensibility
    {
        public string GetCustomUI(string ribbonID)
        {
            return System.IO.File.ReadAllText(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Mail2TicketRibbon.xml"));
        }

        public void OnMail2TicketClicked(Office.IRibbonControl control)
        {
            System.Windows.Forms.MessageBox.Show("Mail2Ticket wurde geklickt!");
            // Hier kann die eigentliche Logik zum Erstellen eines Tickets implementiert werden.
        }
    }
}
