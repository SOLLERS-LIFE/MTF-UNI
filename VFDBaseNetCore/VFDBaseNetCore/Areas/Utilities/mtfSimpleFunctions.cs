using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// http://www.pdfsharp.net/?AspxAutoDetectCookieSupport=1
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

// client to k8s instance of https://thecodingmachine.github.io/gotenberg/
// https://github.com/ChangemakerStudios/GotenbergSharpApiClient
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

namespace MTF.Utilities
{
    public static class mtfSimpleFunctions
    {
        public class memFile
        {
            public string fname { get; set; }
            public byte[] fbody { get; set; }
        }
        public static byte[] ZipFiles(List<memFile> _files)
        {
            using (MemoryStream outStream = new())
            {
                using (ZipArchive archive = new(outStream, ZipArchiveMode.Create, true))
                {
                    foreach (var fl in _files)
                    {
                        var fileInArchive = archive.CreateEntry(fl.fname, CompressionLevel.Optimal);
                        using (Stream entryStream = fileInArchive.Open())
                        {
                            using (MemoryStream fileToCompressStream = new(fl.fbody))
                            {
                                fileToCompressStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
                
                return outStream.ToArray();
            }
        }

        public static string fileNamePartSanitizing (string _inp)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(_inp, "?");
        }

        public class memPdf
        {
            public string fname { get; set; }
            public PdfDocument fbody { get; set; }
        }
        public static byte[] MergePdfs(List<memFile> pdfs)
        {
            string curName = string.Empty;
            try
            {
                List<memPdf> lstDocuments = new List<memPdf>();
                
                foreach (var pdf in pdfs)
                {
                    curName = pdf.fname;
                    lstDocuments.Add(new mtfSimpleFunctions.memPdf
                                         { 
                                            fname = pdf.fname,
                                            fbody = PdfReader.Open(new MemoryStream(pdf.fbody), PdfDocumentOpenMode.Import)
                                         }
                    );
                }

                using (PdfDocument outPdf = new PdfDocument())
                {
                    for (int i = 1; i <= lstDocuments.Count; i++)
                    {
                        curName = lstDocuments[i - 1].fname;
                        foreach (PdfPage page in lstDocuments[i - 1].fbody.Pages)
                        {
                            outPdf.AddPage(page);
                        }
                    }
                    curName = "No specific Certificate";

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    MemoryStream stream = new MemoryStream();
                    outPdf.Save(stream, false);
                    byte[] bytes = stream.ToArray();
                    stream.Close();

                    return bytes;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CertNo: {curName}", ex);
            }
        }

        public static async Task<MemoryStream> buildReport(GotenbergSharpClient _gsc, string _body, string _footer)
        {
            var builder = new HtmlRequestBuilder()
                                 .AddDocument(doc =>
                                     doc.SetBody(_body).SetFooter(GetFooter(_footer))
                                 ).WithDimensions(dims =>
                                 {
                                     dims.SetPaperSize(PaperSizes.A4)
                                         .SetMargins(Margins.Normal)
                                         .LandScape(false);
                                 }).ConfigureRequest(config =>
                                 {
                                     config.ChromeRpccBufferSize(1024);
                                 });
            var req = await builder.BuildAsync();

            MemoryStream dst = new MemoryStream(1 * 1024 * 1024);
            await (await _gsc.HtmlToPdfAsync(req)).CopyToAsync(dst);
            dst.Position = 0;

            return dst;
        }

        public static string GetFooter(string _nm)
        {

            var s = @$"<html>
                        <head>
                            <style>
                                body {{ font-size: 8rem; font-family: Roboto,""Helvetica Neue"",Arial,sans-serif; margin: 4rem auto; }}
                            </style>
                        </head>
	                    <body>
                            <p>Page <span class=""pageNumber""></span> of <span class=""totalPages""> pages</span> PDF Created on <span class=""date""></span> by <span>""{_nm}""</span></p>
                        </body>
                    </html>";
            return s;
        }
    }
}
