using System;
using System.Collections.Generic;

namespace HospitalityArchitect;

public static class FinanceUtil
{
    public static FinanceReport Sum(List<FinanceReport> reports)
    {
        FinanceReport sumReport = new FinanceReport();
        foreach (var financeReport in reports)
        {
            foreach (var booking in financeReport.getBooking())
            {
                if (Enum.TryParse(booking.Key, out FinanceReport.ReportEntryType type))
                    sumReport.recordBooking(type, booking.Value);
            }
        }
        return sumReport;
    }
}