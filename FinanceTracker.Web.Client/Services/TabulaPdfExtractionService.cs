using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.IO;
using Tabula;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace FinanceTracker.Web.Services
{
    public class TabulaPdfExtractionService
    {
        public IReadOnlyList<Tabula.Table> ExtractTables(Stream pdfStream, string bankName)
        {
            // Open the PDF stream using PdfPig's parsing options
            using (PdfDocument document = PdfDocument.Open(pdfStream, new ParsingOptions() { ClipPaths = true }))
            {
                // Extract the target page area (e.g., page 1)
                PageArea page = ObjectExtractor.Extract(document, 1);

                IExtractionAlgorithm algorithm;

                // Route to the correct algorithm based on the visual layout
                if (bankName.ToUpper() == "M&T")
                {
                    // Lattice mode for solid grid lines
                    algorithm = new SpreadsheetExtractionAlgorithm();
                }
                else
                {
                    // Stream mode for whitespace-delimited columns
                    algorithm = new BasicExtractionAlgorithm();
                }

                // Extract and return the detected tables
                return algorithm.Extract(page);
            }
        }
    }
}