using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class CommunicationSettingsViewModel
    {
        // Email Settings
        public EmailMaster EmailMaster { get; set; } = new EmailMaster();
        public List<EmailMaster> EmailList { get; set; } = new List<EmailMaster>();

        // WhatsApp Settings
        public WhatsAppSettings WhatsAppSetting { get; set; } = new WhatsAppSettings();
        public List<WhatsAppSettings> WhatsAppList { get; set; } = new List<WhatsAppSettings>();

        // Dropdown Lists
        public List<string> EmailStatusList { get; set; } = new List<string>();
        public List<string> PurposeList { get; set; } = new List<string>();
        public List<string> ServiceList { get; set; } = new List<string>();
    }
}