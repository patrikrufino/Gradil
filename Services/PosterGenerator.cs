using Gradil.Models;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Gradil.Services
{
    // Coordinates the poster generation flow using PdfSharpCore only.
    // This implementation cuts pages logically (vector-aware) by drawing
    // the source page form and cropping via source rectangles.
    public class PosterGenerator
    {
        public PosterGenerator()
        {
        }

        // Generates a tiled poster PDF and returns the output file path.
        public async Task<string> GenerateAsync(TiledPosterOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.SourceFile) || !File.Exists(options.SourceFile))
                throw new FileNotFoundException("Arquivo PDF fonte não encontrado.", options.SourceFile);

            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputDir = Path.Combine(docs, "Gradil");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string name = "gradil_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".pdf";
            string outputPath = Path.Combine(outputDir, name);

            // Open source document (import mode)
            var input = PdfReader.Open(options.SourceFile, PdfDocumentOpenMode.Import);

            var outputPdf = new PdfDocument();

            // Use XPdfForm to draw pages as forms and crop by source rectangle
            using (var xform = XPdfForm.FromFile(options.SourceFile))
            {
                for (int pageIndex = 0; pageIndex < input.PageCount; pageIndex++)
                {
                    var srcPage = input.Pages[pageIndex];

                    double pageWidth = srcPage.Width.Point; // points
                    double pageHeight = srcPage.Height.Point;

                    double pieceWidth = pageWidth / options.Cols;
                    double pieceHeight = pageHeight / options.Rows;

                    xform.PageNumber = pageIndex + 1; // XPdfForm pages are 1-based

                    for (int r = 0; r < options.Rows; r++)
                    {
                        for (int c = 0; c < options.Cols; c++)
                        {
                            double x0 = c * pieceWidth;
                            double y0 = r * pieceHeight;

                            var newPage = outputPdf.AddPage();
                            newPage.Width = XUnit.FromPoint(pieceWidth);
                            newPage.Height = XUnit.FromPoint(pieceHeight);

                            using (var gfx = XGraphics.FromPdfPage(newPage))
                            {
                                // Draw the full source page at a negative offset so the
                                // desired piece (x0,y0 -> x0+pieceWidth,y0+pieceHeight)
                                // appears on the smaller output page without scaling.
                                // We draw with the source page size (pageWidth,pageHeight)
                                // so the drawn content is not resized.
                                double destX = -x0;
                                double destY = -y0;
                                double destW = pageWidth;
                                double destH = pageHeight;

                                gfx.DrawImage(xform, destX, destY, destW, destH);
                            }

                            // no inline preview generation (rendering not supported by PdfSharpCore)
                        }
                    }
                }
            }

            // Save output
            await Task.Run(() => outputPdf.Save(outputPath));

            return outputPath;
        }
    }
}
