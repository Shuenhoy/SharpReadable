using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;
using SharpReadable;
using CommandLine;
using ShellProgressBar;

Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {

                       using (PdfDocument document = PdfDocument.Open(o.Input))
                       {
                           PdfDocumentBuilder builder = new PdfDocumentBuilder();

                           PdfSharpCore.Pdf.PdfDocument pdf = PdfSharpCore.Pdf.IO.PdfReader.Open(o.Input, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Modify);
                           IMask mask;
                           if (o.Mask == "heuristic")
                           {
                               mask = new HeuristicMask();
                           }
                           else if (o.Mask == "http")
                           {
                               mask = new HttpMask(o.APIUrl, o.Refine, o.Fill, o.ContextSize);
                           }
                           else
                           {
                               throw new Exception("Unknown mask type");
                           }


                           var options = new ProgressBarOptions
                           {
                               ProgressCharacter = '=',
                               ProgressBarOnBottom = true
                           };

                           using var pbar = new ProgressBar(document.NumberOfPages - o.Skip - o.SkipLast, "Processing", options);

                           foreach (var (pig, sharp) in
                                   Enumerable.Zip<Page, PdfSharpCore.Pdf.PdfPage>(document.GetPages(), pdf.Pages)
                                   .Skip(o.Skip)
                                   .SkipLast(o.SkipLast))
                           {
                               PdfSharpCore.Drawing.XGraphics? renderer = PdfSharpCore.Drawing.XGraphics.FromPdfPage(sharp);
                               var color = PdfSharpCore.Drawing.XColor.FromArgb(127, 255, 255, 255);
                               var words = pig.GetWords();
                               var pen = new PdfSharpCore.Drawing.XPen(color, 0);
                               var brush = new PdfSharpCore.Drawing.XSolidBrush(color);

                               var text = String.Join(" ", words.Select(x => x.Text));
                               var maskArray = mask.GetMask(text);
                               var wordCounts = words.Count();
                               if (wordCounts > 0)
                               {
                                   var wordPositions = new int[words.Count()];

                                   wordPositions[0] = 0;
                                   foreach (var (i, word) in words.SkipLast(1).Select((v, i) => (i, v)))
                                   {
                                       wordPositions[i + 1] = wordPositions[i] + word.Text.Length + 1;
                                   }

                                   foreach (var (word, pos) in Enumerable.Zip(words, wordPositions))
                                   {
                                       foreach (var (notmasked, letter) in Enumerable.Zip(maskArray.Skip(pos), word.Letters))
                                       {
                                           if (!notmasked)
                                           {
                                               renderer.DrawRectangle(pen, brush, letter.GlyphRectangle.Left, sharp.Height - letter.GlyphRectangle.Top, letter.GlyphRectangle.Width, letter.GlyphRectangle.Height);
                                           }
                                       }
                                   }
                               }
                               pbar.Tick($"Page {pig.Number} done");

                           }

                           pdf.Save(o.Output);
                       }
                   });

