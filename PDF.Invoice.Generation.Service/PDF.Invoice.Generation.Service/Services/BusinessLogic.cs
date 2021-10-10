using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PDF.Invoice.Generation.Service.Model;

namespace PDF.Invoice.Generation.Service.Services
{
    public class BusinessLogic
    {
        private readonly ReceiptDetails _receiptDetails;
        private readonly ServiceDescription _serviceDescription;
        private readonly InvoiceCalculation _invoiceCalculation;
        private readonly List<string> _listOfServiceRenderedDescriptions = new List<string>();
        private readonly List<decimal> _listOfWorkingHours = new List<decimal>();

        public BusinessLogic(ReceiptDetails receiptDetails,
            ServiceDescription serviceDescription, InvoiceCalculation invoiceCalculation)
        {
            _receiptDetails = receiptDetails;
            _serviceDescription = serviceDescription;
            _invoiceCalculation = invoiceCalculation;
        }

        public string FileNameGenerator()
        {
            string monthName =
                CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(_serviceDescription.InvoiceMonth);
            string contractorName = Regex.Replace(_receiptDetails.Contact, "[^a-zA-Z]+", "_");

            StringBuilder invoiceFileName = new StringBuilder();
            invoiceFileName.Append("Invoice");
            invoiceFileName.Append('-');
            invoiceFileName.Append(monthName);
            invoiceFileName.Append('-');
            invoiceFileName.Append(_serviceDescription.InvoiceYear);
            invoiceFileName.Append('-');
            invoiceFileName.Append(contractorName);
            invoiceFileName.Append(".pdf");

            return invoiceFileName.ToString();
        }

        public List<string> ServiceRenderedDetails()
        {
            var firstDayOfMonth = new DateTime(_serviceDescription.InvoiceYear, _serviceDescription.InvoiceMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            if (firstDayOfMonth.DayOfWeek != DayOfWeek.Saturday && firstDayOfMonth.DayOfWeek != DayOfWeek.Sunday)
            {
                var diff = 7 - (int) firstDayOfMonth.DayOfWeek;

                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth, firstDayOfMonth.AddDays(diff)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(diff + 1),
                    firstDayOfMonth.AddDays(diff + 7)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(diff + 8),
                    firstDayOfMonth.AddDays(diff + 14)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(diff + 15),
                    firstDayOfMonth.AddDays(diff + 21)));
                if (firstDayOfMonth.AddDays(diff + 22) == lastDayOfMonth)
                {
                    _listOfServiceRenderedDescriptions.Add(ServiceOnText(firstDayOfMonth.AddDays(diff + 22)));
                }
                else if (firstDayOfMonth.AddDays(diff + 21) != lastDayOfMonth)
                {
                    _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(diff + 22),
                        lastDayOfMonth));
                }
            }
            else
            {
                // if 1st of the Month is SATURDAY OR SUNDAY, then its ignored
                var init = firstDayOfMonth.DayOfWeek == DayOfWeek.Saturday ? 1 : 0;
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(init + 1),
                    firstDayOfMonth.AddDays(init + 7)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(init + 8),
                    firstDayOfMonth.AddDays(init + 14)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(init + 15),
                    firstDayOfMonth.AddDays(init + 21)));
                _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(init + 22),
                    firstDayOfMonth.AddDays(init + 28)));
                if (firstDayOfMonth.AddDays(init + 29) == lastDayOfMonth)
                {
                    _listOfServiceRenderedDescriptions.Add(ServiceOnText(firstDayOfMonth.AddDays(init + 29)));
                }
                else if (firstDayOfMonth.AddDays(init + 28) != lastDayOfMonth)
                {
                    _listOfServiceRenderedDescriptions.Add(ServiceFromText(firstDayOfMonth.AddDays(init + 29),
                        lastDayOfMonth));
                }
            }

            return _listOfServiceRenderedDescriptions;
        }

        private string ServiceFromText(DateTime fromDate, DateTime toDate)
        {
            StringBuilder serviceRenderedText = new StringBuilder();
            serviceRenderedText.Append("For services rendered from ");
            serviceRenderedText.Append(fromDate.ToString("dd MMM "));
            serviceRenderedText.Append(_serviceDescription.InvoiceYear);
            serviceRenderedText.Append(" to ");
            serviceRenderedText.Append(toDate.ToString("dd MMM "));
            serviceRenderedText.Append(_serviceDescription.InvoiceYear);
            return serviceRenderedText.ToString();
        }

        private string ServiceOnText(DateTime date)
        {
            return "For services rendered on " + date.ToString("dd MMM ") + _serviceDescription.InvoiceYear;
        }

        public List<decimal> ListOfWorkingHours()
        {
            _listOfWorkingHours.Add(_serviceDescription.Week_1_Hours_Worked);
            _listOfWorkingHours.Add(_serviceDescription.Week_2_Hours_Worked);
            _listOfWorkingHours.Add(_serviceDescription.Week_3_Hours_Worked);
            _listOfWorkingHours.Add(_serviceDescription.Week_4_Hours_Worked);
            _listOfWorkingHours.Add(_serviceDescription.Week_5_Hours_Worked);
            return _listOfWorkingHours;
        }

        public decimal TotalHours()
        {
            return _serviceDescription.Week_1_Hours_Worked + _serviceDescription.Week_2_Hours_Worked +
                   _serviceDescription.Week_3_Hours_Worked + _serviceDescription.Week_4_Hours_Worked +
                   _serviceDescription.Week_5_Hours_Worked;
        }

        public decimal TotalAmountBeforeGst()
        {
            return TotalHours() * _invoiceCalculation.HourlyRate;
        }

        public decimal CalculatedGst()
        {
            return (TotalAmountBeforeGst() * _invoiceCalculation.GSTPercentage) / 100;
        }

        public decimal SubTotal()
        {
            return TotalAmountBeforeGst() + CalculatedGst();
        }

        public decimal CalculatedRwt()
        {
            return (TotalAmountBeforeGst() * _invoiceCalculation.RWTPercentage) / 100;
        }

        public decimal TotalSalary()
        {
            return SubTotal() - CalculatedRwt();
        }
    }
}