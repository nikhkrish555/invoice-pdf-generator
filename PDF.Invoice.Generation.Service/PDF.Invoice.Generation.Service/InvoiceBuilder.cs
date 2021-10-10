using System;
using System.Collections.Generic;
using System.Text;
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Enumerations;
using Gehtsoft.PDFFlow.Models.Shared;
using Gehtsoft.PDFFlow.UserUtils;
using Gehtsoft.PDFFlow.Utils;
using PDF.Invoice.Generation.Service.Model;
using PDF.Invoice.Generation.Service.Services;

namespace PDF.Invoice.Generation.Service.Service
{
    public class InvoiceBuilder
    {
        private readonly ConsultantAddress _consultantAddress;
        private readonly ReceiptDetails _receiptDetails;
        private readonly InvoiceCalculation _invoiceCalculation;
        private readonly ContractorAddress _contractorAddress;
        private readonly BusinessLogic _businessLogic;
        private List<string> _listOfServiceRenderedDescriptions = new List<string>();
        private List<decimal> _listOfWorkingHours = new List<decimal>();

        internal const PageOrientation Orientation 
            = PageOrientation.Portrait;
        
        internal static readonly Box Margins  = new Box(50, 70, 50, 20);
        
        internal static readonly FontBuilder FNT10B = 
            Fonts.Helvetica(10f).SetBold();
        
        internal static readonly FontBuilder FNT10 = Fonts.Helvetica(10f);
        
        internal static readonly XUnit PageWidth = 
            (PredefinedSizeBuilder.ToSize(PaperSize.Letter).Width -
             (Margins.Left + Margins.Right));


        public InvoiceBuilder(ConsultantAddress consultantAddress, ReceiptDetails receiptDetails,
             InvoiceCalculation invoiceCalculation,
            ContractorAddress contractorAddress, BusinessLogic businessLogic)
        {
            _consultantAddress = consultantAddress;
            _receiptDetails = receiptDetails;
            _invoiceCalculation = invoiceCalculation;
            _contractorAddress = contractorAddress;
            _businessLogic = businessLogic;
        }

        internal DocumentBuilder Build()
        {
            DocumentBuilder documentBuilder = DocumentBuilder.New();
            var sectionBuilder = documentBuilder.AddSection();
            sectionBuilder
                .SetOrientation(Orientation)
                .SetMargins(Margins);
            sectionBuilder.AddHeaderToBothPages(70, BuildHeader);
            BuildServiceDescriptionInfo(sectionBuilder);
            BuildInvoiceCalculationInfo(sectionBuilder);
            BuildConsultantAddressInfo(sectionBuilder);
            return documentBuilder;
        }

        private void BuildHeader(RepeatingAreaBuilder builder)
        {
            var tableBuilder = builder.AddTable();
            tableBuilder
                .SetWidth(XUnit.FromPercent(100))
                .SetBorder(Stroke.None)
                .AddColumnPercentToTable("", 72)
                .AddColumnPercentToTable("", 28);
            var rowBuilder = tableBuilder.AddRow();
            var cellBuilder = rowBuilder.AddCell()
                .SetVerticalAlignment(VerticalAlignment.Center)
                .SetPadding(0, 0, 0, 0);
            cellBuilder
                .AddParagraph(_consultantAddress.ConsultantName).SetFont(FNT10B);
            cellBuilder
                .AddParagraph(_consultantAddress.Address1).SetFont(FNT10);
            cellBuilder
                .AddParagraph(_consultantAddress.Address2).SetFont(FNT10);

            cellBuilder = rowBuilder.AddCell()
                .SetVerticalAlignment(VerticalAlignment.Center)
                .SetPadding(0, 0, 0, 0);
            cellBuilder
                .AddParagraph("TAX INVOICE No: " + DateTime.Now.ToString("yyyyMMdd")).SetFont(FNT10B);
            cellBuilder
                .AddParagraph("GST: " + _receiptDetails.GSTNumber).SetFont(FNT10);
            cellBuilder
                .AddParagraph("Date: " + DateTime.Now.ToString("dd-MMM-yyyy")).SetFont(FNT10);
            cellBuilder
                .AddParagraph("Contact: " + _receiptDetails.Contact).SetFont(FNT10B);
        }

        private void BuildServiceDescriptionInfo(SectionBuilder sectionBuilder)
        {
            sectionBuilder
                .AddParagraph($"{"Description",0}{"Hours",158}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B)
                .SetMarginTop(22);
            sectionBuilder.AddLine(PageWidth, 2f, Stroke.Solid);
            FillServiceInfoTable(sectionBuilder.AddTable());
            TotalServiceRenderedHours(sectionBuilder);
        }

        private void FillServiceInfoTable(TableBuilder tableBuilder)
        {
            tableBuilder
                .SetWidth(XUnit.FromPercent(100))
                .SetBorder(Stroke.None)
                .SetContentRowStyleFont(FNT10)
                .AddColumnPercentToTable("", 94)
                .AddColumnPercentToTable("", 6);

            _listOfServiceRenderedDescriptions = _businessLogic.ServiceRenderedDetails();
            _listOfWorkingHours =  _businessLogic.ListOfWorkingHours();

            for (int i = 0; i < _listOfServiceRenderedDescriptions.Count; i++)
            {
                var rowBuilder = tableBuilder.AddRow();
                var cellBuilder = rowBuilder.AddCell()
                    .SetPadding(0, 3.5f, 0, 3.5f)
                    .SetBorderWidth(0, 0, 0, 0.5f)
                    .SetBorderStroke(Stroke.None, Stroke.None, Stroke.None, Stroke.Solid);

                cellBuilder
                    .AddParagraph(_listOfServiceRenderedDescriptions[i]).SetFont(FNT10);

                cellBuilder = rowBuilder.AddCell()
                    .SetPadding(0, 3.5f, 0, 3.5f)
                    .SetBorderWidth(0, 0, 0, 0.5f)
                    .SetBorderStroke(Stroke.None, Stroke.None, Stroke.None, Stroke.Solid);

                if (_listOfWorkingHours[i].ToString().Length == 4)
                {
                    cellBuilder
                        .AddParagraph("  " + _listOfWorkingHours[i]).SetFont(FNT10);
                }
                else
                {
                    cellBuilder
                        .AddParagraph(_listOfWorkingHours[i].ToString()).SetFont(FNT10);
                }
            }
        }
        
        private void TotalServiceRenderedHours(SectionBuilder sectionBuilder)
        {
            sectionBuilder
                .AddParagraph($"{"Total",0}{_businessLogic.TotalHours(),169}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B);
            
            sectionBuilder.AddLine(PageWidth, 2f, Stroke.Solid).SetColor(Color.Gray).SetMarginTop(32);
        }

        private void BuildInvoiceCalculationInfo(SectionBuilder sectionBuilder)
        {
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.Append("Total ");
            textBuilder.Append(_businessLogic.TotalHours());
            textBuilder.Append(" hours @ ");
            textBuilder.Append(_invoiceCalculation.HourlyRate);
            textBuilder.Append("$ / hour");
            
            sectionBuilder
                .AddParagraph($"{textBuilder,0}{"$"+String.Format("{0:n}", _businessLogic.TotalAmountBeforeGst()),123}") //137
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10)
                .SetMarginTop(15);

            textBuilder.Clear();
            textBuilder.Append("GST ");
            textBuilder.Append(_invoiceCalculation.GSTPercentage);
            textBuilder.Append("% on ");
            textBuilder.Append("$" + String.Format("{0:n}", _businessLogic.TotalAmountBeforeGst()));
            
            sectionBuilder
                .AddParagraph($"{textBuilder,0}{"$"+String.Format("{0:n}", _businessLogic.CalculatedGst()),136}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10)
                .SetMarginTop(10);
            
            sectionBuilder
                .AddParagraph($"{"Subtotal",0}{"$"+String.Format("{0:n}", _businessLogic.SubTotal()),160}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B)
                .SetMarginTop(10);

            textBuilder.Clear();
            textBuilder.Append("RWT ");
            textBuilder.Append(_invoiceCalculation.RWTPercentage);
            textBuilder.Append("% on ");
            textBuilder.Append("$" + String.Format("{0:n}", _businessLogic.TotalAmountBeforeGst()));
            
            sectionBuilder
                .AddParagraph($"{textBuilder,0}{"-$"+String.Format("{0:n}", _businessLogic.CalculatedRwt()),135}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10)
                .SetMarginTop(10);
            
            sectionBuilder
                .AddParagraph($"{"Total",0}{"$"+String.Format("{0:n}", _businessLogic.TotalSalary()),166}")
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B)
                .SetMarginTop(10);
            
            sectionBuilder.AddLine(PageWidth, 2f, Stroke.Solid).SetColor(Color.Gray).SetMarginTop(15);
        }

        private void BuildConsultantAddressInfo(SectionBuilder sectionBuilder)
        {
            sectionBuilder
                .AddParagraph(_contractorAddress.CompanyName)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B)
                .SetMarginTop(50);
            
            sectionBuilder
                .AddParagraph(_contractorAddress.Address1)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10);
            
            sectionBuilder
                .AddParagraph(_contractorAddress.Address2)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10);
            
            sectionBuilder
                .AddParagraph(_contractorAddress.Phone)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10);
            
            sectionBuilder
                .AddParagraph(_contractorAddress.Email)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10);

            sectionBuilder
                .AddParagraph("Account Number: " + _contractorAddress.BankAccountNumber)
                .SetAlignment(HorizontalAlignment.Left)
                .SetFont(FNT10B);
        }
    }
}