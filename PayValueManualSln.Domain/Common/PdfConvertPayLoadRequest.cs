using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Common
{
    public class PdfConvertPayLoadRequest
    {
        public int ConverterTypeId { get; set; }
        public string OutputFolderName { get; set; }
        public string DocumentName { get; set; }
        public string HtmlContentOrUrlPath { get; set; }
        public string PaperSize { get; set; }
        public string Orientation { get; set; }
    }

    public class PdfConverterRequest
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string PaperSize { get; set; } = string.Empty;
        public string Orientation { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public string OutputFolderName { get; set; } = string.Empty;
        public bool DuplicatedCopy { get; set; } = false;
        public bool PagesCount => DuplicatedCopy == true;
        public bool SaveInFolder { get; set; } = false;
    }
}
