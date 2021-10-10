using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PDF.Invoice.Generation.Service.Model;
using PDF.Invoice.Generation.Service.Service;
using PDF.Invoice.Generation.Service.Services;

namespace PDF.Invoice.Generation.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            ServiceProvider serviceProvider = CreateServiceProvier();
            InvoiceRunner invoiceRunner = serviceProvider.GetService<InvoiceRunner>();
            
            string fileName = invoiceRunner.GetFileName();
            Parameters parameters = new Parameters(fileName);
            
            if (!PrepareParameters(parameters))
            {
                return 1;
            }
            
            invoiceRunner.Run().Build(parameters.file);
            
            Console.WriteLine("\"" + Path.GetFullPath(fileName) 
                            + "\" document has been successfully built.");
            return 0;
        }

        private static IConfigurationBuilder CreateConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .SetFileProvider(new PhysicalFileProvider(Environment.CurrentDirectory))
                .AddJsonFile("appsettings.json");
        }

        private static ServiceProvider CreateServiceProvier()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = CreateConfigurationBuilder().Build();
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<InvoiceRunner>();
            serviceCollection.AddScoped<BusinessLogic>();
            serviceCollection.AddScoped<InvoiceBuilder>();
            
            
            serviceCollection.Configure<ConsultantAddress>(configuration.GetSection("ConsultantAddress"));
            serviceCollection.Configure<ReceiptDetails>(configuration.GetSection("ReceiptDetails"));
            serviceCollection.Configure<ServiceDescription>(configuration.GetSection("ServiceDescription"));
            serviceCollection.Configure<InvoiceCalculation>(configuration.GetSection("InvoiceCalculation"));
            serviceCollection.Configure<ContractorAddress>(configuration.GetSection("ContractorAddress"));
            
            serviceCollection.TryAddSingleton<ConsultantAddress>(provider =>
                provider.GetRequiredService<IOptions<ConsultantAddress>>().Value);
            serviceCollection.TryAddSingleton<ReceiptDetails>(provider =>
                provider.GetRequiredService<IOptions<ReceiptDetails>>().Value);
            serviceCollection.TryAddSingleton<ServiceDescription>(provider =>
                provider.GetRequiredService<IOptions<ServiceDescription>>().Value);
            serviceCollection.TryAddSingleton<InvoiceCalculation>(provider =>
                provider.GetRequiredService<IOptions<InvoiceCalculation>>().Value);
            serviceCollection.TryAddSingleton<ContractorAddress>(provider =>
                provider.GetRequiredService<IOptions<ContractorAddress>>().Value);
                
            return serviceCollection.BuildServiceProvider();
        }

        private static bool PrepareParameters(Parameters parameters)
        {
            if (File.Exists(parameters.file))
            {
                try
                {
                    File.Delete(parameters.file);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Can't delete file: " + 
                                            Path.GetFullPath(parameters.file));
                    Console.Error.WriteLine(e.Message);
                    return false;
                }
            }
        
            return true;
        }
        
        internal class Parameters
        {
            public string file;
            public Parameters(string file)
            {
                this.file = file;
            }
        }
    }
}
