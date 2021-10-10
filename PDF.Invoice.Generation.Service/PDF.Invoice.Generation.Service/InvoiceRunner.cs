using Gehtsoft.PDFFlow.Builder;
using PDF.Invoice.Generation.Service.Service;
using PDF.Invoice.Generation.Service.Services;

namespace PDF.Invoice.Generation.Service
{
    public class InvoiceRunner
    {
        private readonly InvoiceBuilder _invoiceBuilder;
        private readonly BusinessLogic _businessLogic;
        
        public InvoiceRunner(InvoiceBuilder invoiceBuilder, BusinessLogic businessLogic)
        {
            _invoiceBuilder = invoiceBuilder;
            _businessLogic = businessLogic;
        }
        public DocumentBuilder Run()
        {
            return _invoiceBuilder.Build();
        }

        public string GetFileName()
        {
            return _businessLogic.FileNameGenerator();
        }
    }
}