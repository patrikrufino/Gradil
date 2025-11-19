using System.Drawing;
using System.Threading.Tasks;

namespace Gradil.Services
{
    // Abstraction for PDF operations (open, render, close).
    // Implement using PdfiumSharp or another PDF library.
    public interface IPdfService
    {
        Task OpenAsync(string path);
        int PageCount { get; }
        SizeF GetPageSize(int pageIndex);
        Task<Bitmap> RenderPageAreaAsync(int pageIndex, RectangleF area, float dpi = 150f);
        void Close();
    }
}
