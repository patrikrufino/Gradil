using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Gradil.Services
{
    // Stub implementation kept for compatibility. We now use PdfSharpCore-only
    // flow via PosterGenerator. These methods intentionally throw to make it
    // obvious this service is not used in the current pipeline.
    public class PdfiumPdfService : IPdfService
    {
        public int PageCount => throw new NotImplementedException();

        public Task OpenAsync(string path)
        {
            throw new NotImplementedException("PdfiumPdfService is not used in PdfSharpCore flow.");
        }

        public SizeF GetPageSize(int pageIndex)
        {
            throw new NotImplementedException();
        }

        public Task<Bitmap> RenderPageAreaAsync(int pageIndex, RectangleF area, float dpi = 150f)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            // no-op
        }
    }
}
