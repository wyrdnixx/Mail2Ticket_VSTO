using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Office.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using Window = System.Windows.Window;

namespace Mail2Ticket
{
    [ComVisible(true)]
    public class Mail2TicketRibbon : Office.IRibbonExtensibility
    {
        public string GetCustomUI__OLD(string ribbonID)
        {
            return System.IO.File.ReadAllText(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Mail2TicketRibbon.xml"));
        }

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("Mail2Ticket.Mail2TicketRibbon.xml");
        }

        private string GetResourceText(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }



        public void OnMail2TicketClicked(Office.IRibbonControl control)
        {
            var dialog = new TicketDialog();
            // Aktuell ausgewählte Mail holen
            Outlook.Application app = Globals.ThisAddIn.Application;
            Outlook.Selection selection = app.ActiveExplorer().Selection;
            
            if (selection.Count > 0 && selection[1] is Outlook.MailItem mail)
            {
                //sender = mail.SenderName + " <" + mail.SenderEmailAddress + ">";
                string emailSuject = mail.Subject;
                
                dialog.StartMail2Ticket(mail);
               dialog.lblVersionInfo.Content = $"Mail2Ticket Version: {Assembly.GetExecutingAssembly().GetName().Version}";

                var window = new Window
                {
                    Title = "Ticket erstellen",
                    Content = dialog,
                    Width = 1050,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,

                };
                window.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("Bitte wählen Sie eine E-Mail aus, um ein Ticket zu erstellen.", "Keine E-Mail ausgewählt", MessageBoxButton.OK, MessageBoxImage.Warning);
            }



        }


    }
}
