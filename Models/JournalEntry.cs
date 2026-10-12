using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }

        public string EntryType { get; set; } = "Journal Entry";
        public string FromTemplate { get; set; }
        public string Series { get; set; } = "ACC-JV-.YYYY.-";
        public string Company { get; set; } = "AITS";
        public string CompanyGstin { get; set; }

        public DateTime PostingDate { get; set; } = DateTime.Now;

        public string ReferenceNumber { get; set; }
        public DateTime? ReferenceDate { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal DifferenceAmount { get; set; }

        public bool MultiCurrency { get; set; }

        public string UserRemark { get; set; }

        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string PayToRecdFrom { get; set; }
        public string LetterHead { get; set; }
        public string PrintHeading { get; set; }

        public string ModeOfPayment { get; set; }
        public string IsOpening { get; set; } = "No";
        public string Title { get; set; }

        public string ErpNextId { get; set; }

        public List<JournalEntryAccount> Accounts { get; set; } = new List<JournalEntryAccount>();
    }

    public class JournalEntryAccount
    {
        public int Id { get; set; }
        public int JournalEntryId { get; set; }

        public string Account { get; set; }
        public string PartyType { get; set; }
        public string Party { get; set; }
        public string ModeOfPayment { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class JournalEntryListRow
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string EntryType { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public DateTime PostingDate { get; set; }
    }
}