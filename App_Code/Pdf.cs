using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;
using Igprog;

/// <summary>
/// Pdf
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Pdf : System.Web.Services.WebService {
    iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.BLACK, Element.ALIGN_LEFT, 1);
    Global g = new Global();
    BuisinessUnit bu = new BuisinessUnit();
    public Pdf() {
    }

    [WebMethod]
    public string Loans(int month, int year, string buisinessUnitCode, Loan.Loans loans) {
        try {
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
            Rectangle ps = PageSize.A4;
            Document doc = new Document(ps.Rotate());
            string path = Server.MapPath("~/upload/pdf/temp/");
            g.DeleteFolder(path);
            g.CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("KUP: {0} / {1}/{2}", buisinessUnitCode, month, year), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Mat br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Odobrena pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Manipulativni troškovi", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Neotplaćena pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Za isplatu", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            foreach (Loan.NewLoan x in loans.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.user.id, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(g.Currency(x.loan), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Currency(x.manipulativeCosts), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(g.Currency(x.actualLoan), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase(g.Currency(x.restToRepayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell6.Border = 0;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell7.Border = 0;
                table.AddCell(cell7);
            }
            doc.Add(table);

            table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loans.total.loan), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loans.total.manipulativeCosts), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loans.total.actualLoan), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loans.total.restToRepayment), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("TOOD", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Card(int year, User.NewUser user) {
        try {
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
            Rectangle ps = PageSize.A4;
            Document doc = new Document(ps.Rotate());
            string path = Server.MapPath("~/upload/pdf/temp/");
            g.DeleteFolder(path);
            g.CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("{0} {1}", user.lastName, user.firstName), GetFont(12, Font.BOLD)));
            p.Add(new Paragraph(string.Format("{0} god", year), GetFont(10, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Datum", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Mjesec", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Opis", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Ulog", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Duguje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Potražuje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
          
            foreach (Account.NewAccount x in user.records) {
                PdfPCell cell1 = new PdfPCell(new Phrase("TODO", GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase("TODO", GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(x.note, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell6.Border = 0;
                table.AddCell(cell6);
            }
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Suspension(int month, int year, string buisinessUnitCode, Account.Accounts records) {
        try {
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
            Rectangle ps = PageSize.A4;
            Document doc = new Document();
            string path = Server.MapPath("~/upload/pdf/temp/");
            g.DeleteFolder(path);
            g.CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("KUP - obustava na Plaći za {0}/{1} {2}", g.Month(month), year, bu.Get(buisinessUnitCode).title), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("M. br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Šifra", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Ulog", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Rata", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Ukupno", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
          
            foreach (Account.NewAccount x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.user.id, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("TODO", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell6.Border = 0;
                table.AddCell(cell6);
            }
            doc.Add(table);

            table = new PdfPTable(6);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.monthlyFee), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.repayment), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.totalObligation), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Recapitulation(int month, Account.RecapYearlyTotal records, string title) {
        try {
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
            Rectangle ps = PageSize.A4;
            Document doc = new Document(ps.Rotate());
            string path = Server.MapPath("~/upload/pdf/temp/");
            g.DeleteFolder(path);
            g.CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("{0}", title), GetFont(12, Font.BOLD)));
            p.Add(new Paragraph(string.Format("{0} god.", records.year), GetFont(10, Font.NORMAL)));
            p.Add(new Paragraph(string.Format("Konto: {0}", records.account), GetFont(10, Font.NORMAL)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 4f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Datum", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Sadržaj", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Duguje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Potražuje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
          
            foreach (Account.RecapMonthlyTotal x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.total.date, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.month, x.total.note), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.output), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.input), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
            }
            doc.Add(table);

            table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 4f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", records.total.output), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", records.total.input), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    private Font GetFont(int size, int style) {
       return FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, size, style);
    }

    private Font GetFont() {
        return GetFont(9, 0); // Normal font
    }

    private Font GetFont(int size) {
        return GetFont(size, 0);
    }

    private Font GetFont(bool x) {
        return GetFont(9, x == true ? Font.BOLD: Font.NORMAL);
    }
    private Font GetFont(int size, bool x) {
        return GetFont(size, x == true ? Font.BOLD : Font.NORMAL);
    }


}
