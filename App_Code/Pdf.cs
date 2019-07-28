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
                PdfPCell cell1 = new PdfPCell(new Phrase(x.repaymentDate, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(g.Month(x.month), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(x.note, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Currency(x.monthlyFee), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(g.Currency(x.loan), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase(g.Currency(x.restToRepayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
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
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Currency(x.monthlyFee), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(g.Currency(x.repayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase(g.Currency(x.totalObligation), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
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



//    private string ItemDes(Orders.Item item,  string lang) {
//        string des = string.Format("{0} - {1} {2}, {3}, {4}"
//            , item.style != null ? item.style.ToUpper() : ""
//            , item.brand != null ? item.brand.ToUpper() : ""
//            , lang == "hr" ? (item.shortdesc_hr != null ? item.shortdesc_hr.ToUpper() : item.shortdesc_en.ToUpper()) : item.shortdesc_en != null ? item.shortdesc_en.ToUpper() : ""
//            , t.Tran(item.color, lang).ToUpper()
//            , item.size.ToUpper());
//        return des;
//    }

//    private void AppendFooter(Document doc, float spacing, bool invoice) {
//        PdfPTable table = new PdfPTable(2);
//        table.SpacingBefore = spacing;
//        table.WidthPercentage = 100f;
//        //  float[] sign_widths = new float[] { 4f, 1f };
//        // table3.SetWidths(sign_widths);
//        table.AddCell(new PdfPCell(new Phrase("Zahvaljujemo na Vašem povjerenju.", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}",  invoice ? "Oznaka operatera: MIRZA" : "Obračunao(la): Mirza Hodžić"), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

//        doc.Add(table);

//        string footer = @"
//Lateralus j.d.o.o. jednostavno društvo sa ograničenom odgovornošću sa sjedištem u Kastvu, registrirano na Trgovačkom sudu u Rijeci sa temeljnim kapitalom: 200,00kn, uplaćenim u cijelosti. Osnivač društva: Mirza Hodžić. Društvo zastupa: Azra Hodžić, zastupa samostalno i neograničeno.
//Matični broj društva: 040297018, OIB: 90780660216
//Transakcijski računi (IBAN): ERSTE- HR 44 2402006 11 00 64 77 60; ERSTE - HR30 2402006 15 00 05 73 46 (SWIFT/BIC: ESBCHR22 / BANK: Erste & Steiermärkische Bank d.d.)";

//        Paragraph p = new Paragraph();
//        p.Add(new Chunk(footer, GetFont()));
//        doc.Add(p);
//    }

//    private void AppendHeader(Document doc, string lang) {
//        Image logo = Image.GetInstance(logoPath);
//        logo.Alignment = Image.ALIGN_RIGHT;
//        //logo.ScaleToFit(160f, 30f);
//        logo.ScaleToFit(280f, 70f);
//        logo.SpacingAfter = 15f;
//        //logo.ScalePercent(9f);

//        Admin.CompanyInfo ci = JsonConvert.DeserializeObject<Admin.CompanyInfo>(f.GetFile("json", "companyinfo"));

//        Paragraph p = new Paragraph();
//        p.Add(new Chunk(ci.company, GetFont(12, Font.BOLD)));
//        p.Alignment = Element.ALIGN_RIGHT;
//        doc.Add(p);

//        string info = string.Format(@"
//        {0}
//        {1} {2}, {3} {4}: {5}
//        IBAN: {6}
//        IBAN: {7}
//        TEL: {8}
//        MAIL: {9}"
//, ci.companylong
//, ci.zipCode, ci.city, ci.address, t.Tran("pin", lang).ToUpper(), ci.pin
//, ci.iban, ci.iban1
//, ci.phone
//, ci.email
//);

//        PdfPTable header_table = new PdfPTable(2);
//        header_table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, Padding = 0, VerticalAlignment = PdfCell.ALIGN_BOTTOM });
//        header_table.AddCell(new PdfPCell(new Phrase(info, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        header_table.WidthPercentage = 100f;
//        float[] header_widths = new float[] { 1f, 1f };
//        header_table.SetWidths(header_widths);
//        doc.Add(header_table);
//    }


//    private void SavePdf(Orders.NewOrder x, string pdfTempPath, string type) {
//        try {
//            int year = DateTime.Now.Year; // x.year;
//            string number = null;
//            switch(type) {
//                case "offer": number = x.orderId; break;
//                case "invoice": number = x.invoiceId; break;
//                default: number = "xxx"; break;
//            }

//            string fileName = string.Format("{0}", number);
//            string pdfDir = string.Format("~/upload/{0}/", type);
//            string pdfPath = Server.MapPath(string.Format("{0}{1}.pdf", pdfDir, fileName));

//            if (!Directory.Exists(Server.MapPath(pdfDir))) {
//                Directory.CreateDirectory(Server.MapPath(pdfDir));
//            }
//            File.Copy(pdfTempPath, pdfPath, true);
//        } catch (Exception e) { }
//    }

//    private void AppendTblHeader(PdfPTable table) {
//        string price_title = string.Format(@"Cijena
//(bez PDV-a)");
//        string total_title = string.Format(@"Ukupno
//(bez PDV-a)");
//        table.AddCell(new PdfPCell(new Paragraph("Rb", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph("Proizvod / usluga", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph("Količina", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph("j.m.", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph(price_title, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph("Rabat%", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph("PDV%", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//        table.AddCell(new PdfPCell(new Paragraph(total_title, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfCell.ALIGN_MIDDLE, BackgroundColor = Color.LIGHT_GRAY });
//    }

//    private void AppendTblRow(PdfPTable table, int row, Orders.Item item, string lang, double vat) {
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}.", row), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
//        table.AddCell(new PdfPCell(new Phrase(ItemDes(item, lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2 });
//        table.AddCell(new PdfPCell(new Phrase(item.quantity.ToString(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        table.AddCell(new PdfPCell(new Phrase("kom.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", string.Format("{0:N}", item.price)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}%", string.Format("{0:N}", item.discount * 100)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}%", string.Format("{0:N}", vat)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", string.Format("{0:N}", (item.price * item.quantity) - Math.Round((item.price * item.quantity * item.discount),2))), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
//    }


}
