using System;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;
using System.Windows;
using Outlook = Microsoft.Office.Interop.Outlook;

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
            var dialog = new ticketDialog();
            // Aktuell ausgewählte Mail holen
            Outlook.Application app = Globals.ThisAddIn.Application;
            Outlook.Selection selection = app.ActiveExplorer().Selection;
            string sender = "Unbekannt";
            if (selection.Count > 0 && selection[1] is Outlook.MailItem mail)
            {
                //sender = mail.SenderName + " <" + mail.SenderEmailAddress + ">";
                string emailSuject = mail.Subject;
                //var dialog = new ticketDialog();
                dialog.SetMailKontext(mail);
                //dialog.SetEmailSubjectTextbox(emailSuject);

                var window = new Window
                {
                    Title = "Ticket erstellen",
                    Content = dialog,
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                window.ShowDialog();
            }
            else
            {
               MessageBox.Show("Bitte wählen Sie eine E-Mail aus, um ein Ticket zu erstellen.", "Keine E-Mail ausgewählt", MessageBoxButton.OK, MessageBoxImage.Warning);
            }



        }


    }
}
