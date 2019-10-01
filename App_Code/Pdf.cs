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
    string logoPath = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/logo.png"));
    Global g = new Global();
    Settings s = new Settings();
    BuisinessUnit bu = new BuisinessUnit();
    public Pdf() {
    }

    public class PrintDoc {
        public Document doc;
        public string fileName;
    }

    [WebMethod]
    public string NewUser(User.NewUser user) {
        try {
            PrintDoc pd = PreparePrintDoc(false);
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("Mjesto: {0} - {1}", user.buisinessUnit.code, bu.Get(user.buisinessUnit.code).title), GetFont()));
            p.Add(new Paragraph(string.Format("Datum: {0} ", user.accessDate), GetFont()));
            p.Add(new Paragraph(string.Format("IME I PREZIME {0} {1}", user.lastName, user.firstName), GetFont(10)));
            p.Add(new Paragraph(string.Format(@"
"), GetFont()));
            doc.Add(p);

            PdfPTable table = new PdfPTable(1);
            table.AddCell(new PdfPCell(new Phrase("UPRAVNOM ODBORU KASE UZAJAMNE POMOĆI", GetFont(10))) { Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Paragraph(string.Format(@"
Molim naslov da mi odobri upis u članstvo kase uzajamne pomoći."), GetFont()));
            doc.Add(p);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 3f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(potpis člana)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Paragraph(string.Format(@"
Molba primljena dana: {0}", user.accessDate), GetFont()));
            doc.Add(p);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(predsjednik UPRAVNOG ODBORA KUP-a)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 3f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(blagajnik KUP-a)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Loan(Loan.NewLoan loan) {
        try {
            PrintDoc pd = PreparePrintDoc(false);
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("Mjesto: {0} - {1}", loan.user.buisinessUnit.code, bu.Get(loan.user.buisinessUnit.code).title), GetFont()));
            p.Add(new Paragraph(string.Format("Datum: {0} ", loan.loanDate), GetFont()));
            p.Add(new Paragraph(string.Format("IME I PREZIME {0} {1}", loan.user.lastName, loan.user.firstName), GetFont(10)));
            p.Add(new Paragraph(string.Format(@"
"), GetFont()));
            doc.Add(p);

            PdfPTable table = new PdfPTable(1);
            table.AddCell(new PdfPCell(new Phrase("UPRAVNOM ODBORU KASE UZAJAMNE POMOĆI", GetFont(10))) { Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Paragraph(string.Format(@"
Molim naslov da mi odobri pozajmicu u iznosu od {0}.", g.Currency(loan.loan)), GetFont()));
            doc.Add(p);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 2f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(potpis tražioca pozajmice)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Paragraph(string.Format(@"
Molba primljena dana: {0}", loan.loanDate), GetFont()));

            p.Add(new Paragraph(string.Format(@"
Visinu tražene pozajmice odobrio je UPRAVNI ODBOR KUP-a na svojoj sjednici od ______________ i iznosu od {0}.", g.Currency(loan.loan)), GetFont(9, Font.ITALIC)));

            doc.Add(p);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(predsjednik UPRAVNOG ODBORA KUP-a)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            doc.Add(line);


            table = new PdfPTable(3);
            table.SetWidths(new float[] { 2f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("IZNOS ODOBRENE POZAJMICE", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loan.loan), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} manipulativni troškovi", g.manipulativeCostsPerc()), GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loan.manipulativeCosts), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase("ZA ISPLATU", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(loan.withdraw), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase("Rok vraćanja", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} mj.", loan.dedline), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase("Članski ulog", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", "TODO"), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase("UKUPNO ODBICI", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", "TODO"), GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            doc.Add(table);

            table = new PdfPTable(2);
            table.SetWidths(new float[] { 3f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("(blagajnik KUP-a)", GetFont())) { Border = PdfCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Loans(int month, int year, string buisinessUnitCode, Loan.Loans records) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_CENTER;
            p.Add(new Paragraph(string.Format("KUP {0} / {1}/{2}", buisinessUnitCode, month, year), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Mat br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Odobrena pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0}% manipulativni troškovi", g.manipulativeCostsPerc()), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Neotplaćena pozajmica", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Za isplatu", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            foreach (Loan.NewLoan x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.user.id, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(g.Format(x.loan), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Format(x.manipulativeCosts), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(g.Format(x.actualLoan), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase(g.Format(x.restToRepayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell6.Border = 0;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase(g.Format(x.withdraw), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell7.Border = 0;
                table.AddCell(cell7);
            }
            doc.Add(table);

            table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.loan), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.manipulativeCosts), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.actualLoan), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.restToRepayment), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.withdraw), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Card(int year, User.NewUser user) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_CENTER;
            p.Add(new Paragraph(string.Format("{0} {1}", user.lastName, user.firstName), GetFont(12, Font.BOLD)));
            doc.Add(p);
            p = new Paragraph();
            p.Add(new Paragraph(string.Format("{0} god", year), GetFont(10, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ulog", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Pozajmica", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, PaddingTop = 15 });
            doc.Add(table);

            table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Datum", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2 });
            table.AddCell(new PdfPCell(new Phrase("Mjesec", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2 });
            table.AddCell(new PdfPCell(new Phrase("Opis", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("Duguje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Potražuje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Duguje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Potražuje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 1f, 2f, 1f, 1f, 1f, 1f });
            Account.NewAccount ps = user.records[0];
            table.AddCell(new PdfPCell(new Phrase(ps.lastDayInMonth, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20 });
            table.AddCell(new PdfPCell(new Phrase(ps.month, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20 });
            table.AddCell(new PdfPCell(new Phrase(ps.note, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", ps.monthlyFeeStartBalance), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", ps.loanStartBalance), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 20, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            foreach (Account.NewAccount x in user.records) {
                if (x.lastDayInMonth != "01.01") {
                    PdfPCell cell1 = new PdfPCell(new Phrase(x.lastDayInMonth, GetFont()));
                    cell1.Border = 0;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase(x.month, GetFont()));
                    cell2.Border = 0;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase(x.note, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_LEFT };
                    cell3.Border = 0;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase(x.recordType == g.terminationWithdraw ? string.Format("{0:N}", x.amount) : "", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell4.Border = 0;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase(x.recordType == g.monthlyFee || x.recordType == g.userPayment ? string.Format("{0:N}", x.amount) : "", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell5.Border = 0;
                    table.AddCell(cell5);
                    string cell6Val = null;
                    if (x.recordType == g.repayment) {
                        cell6Val = string.Format("{0:N}", x.restToRepayment);
                    } else if (x.recordType == g.withdraw) {
                        cell6Val = string.Format("{0:N}", x.amount);
                    } else {
                        cell6Val = "";
                    }
                    PdfPCell cell6 = new PdfPCell(new Phrase(cell6Val, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell6.Border = 0;
                    table.AddCell(cell6);
                    PdfPCell cell7 = new PdfPCell(new Phrase(x.recordType == g.repayment || x.recordType == g.manipulativeCosts ? string.Format("{0:N}", x.amount) : "", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell7.Border = 0;
                    table.AddCell(cell7);
                }
            }
            doc.Add(table);

            table = new PdfPTable(7);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 1f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Format(user.total.terminationWithdraw), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Format(user.total.userPaymentWithMonthlyFeeTotal), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Format(user.total.loanToRepaid), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Format(user.total.repayment), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Repayment(int month, int year, string buisinessUnitCode, Account.Accounts records) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("Otplata pozajmice: {0} / {1}/{2}", buisinessUnitCode, month, year), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Mat br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ugovorena rata", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Uplaćeno", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Otatak za uplatu", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Napomena", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            
            foreach (Account.NewAccount x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.user.id, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(g.Currency(x.repayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Currency(x.repaid), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(g.Currency(x.restToRepayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase(x.note, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell6.Border = 0;
                table.AddCell(cell6);
            }
            doc.Add(table);

            table = new PdfPTable(6);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.repayment), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.totalObligation), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string MonthlyFee(int month, int year, string buisinessUnitCode, Account.Accounts records) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("Mjesečni ulog: {0} / {1}/{2}", buisinessUnitCode, month, year), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Mat br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ugovoreni iznos", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Uplaćeno", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Napomena", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            
            foreach (Account.NewAccount x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.user.id, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(g.Currency(x.user.monthlyFee), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(g.Currency(x.monthlyFee), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(x.note, GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);
            }
            doc.Add(table);

            table = new PdfPTable(5);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.monthlyFee), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string LoanCard(int year, User.NewUser user) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("{0} {1}", user.lastName, user.firstName), GetFont(12, Font.BOLD)));
            p.Add(new Paragraph(string.Format("Mat. br.{0}", "TODO"), GetFont(10, Font.BOLD)));
            p.Add(new Paragraph(string.Format("{0} god", year), GetFont(10, Font.BOLD)));
            p.Add(new Paragraph(string.Format("Obustave: {0}", "TODO"), GetFont(10, Font.BOLD)));
            p.Add(new Paragraph(string.Format("Iznos duga: {0}", "TODO"), GetFont(10, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 3f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("Osnov za knjiženje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("OTPLATA", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("SALDO", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            foreach (Account.NewAccount x in user.records) {
                if(x.recordType == g.repayment) {
                    PdfPCell cell1 = new PdfPCell(new Phrase(string.Format("{0} {1}", g.Month(Convert.ToInt32(x.month)), x.note), GetFont()));
                    cell1.Border = 0;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0:N}", x.amount), GetFont()));
                    cell2.Border = 0;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase(string.Format("{0:N}", x.restToRepayment), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell3.Border = 0;
                    table.AddCell(cell3);
                }
            }
            doc.Add(table);
            doc.Close();
            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Suspension(int month, int year, string buisinessUnitCode, Account.Accounts records) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Add(new Paragraph(string.Format("KUP - obustava na Plaći za {0}/{1} {2}", g.Month(month), year, bu.Get(buisinessUnitCode).title), GetFont(12, Font.BOLD)));
            if(!string.IsNullOrEmpty(buisinessUnitCode)) {
                p.Add(new Paragraph("* Obustava na plaći različita od prethodnog mjeseca", GetFont(9, Font.ITALIC)));
            }
            doc.Add(p);

            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase(string.IsNullOrEmpty(buisinessUnitCode) ? "" : "M. br.", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(string.IsNullOrEmpty(buisinessUnitCode) ? "" : "Prezime i ime", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ulog", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Rata", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Ukupno", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            table = new PdfPTable(5);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f });
            if (!string.IsNullOrEmpty(buisinessUnitCode)) {
                foreach (Account.NewAccount x in records.data) {
                    PdfPCell cell1 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.totalObligation != x.lastMonthObligation ? "*" : "", x.user.id), GetFont(x.totalObligation != x.lastMonthObligation ? true : false)));
                    cell1.Border = 0;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.user.lastName, x.user.firstName), GetFont(x.totalObligation != x.lastMonthObligation ? true : false)));
                    cell2.Border = 0;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase(string.Format("{0:N}", x.monthlyFee), GetFont(x.totalObligation != x.lastMonthObligation ? true : false))) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell3.Border = 0;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase(string.Format("{0:N}", x.repayment), GetFont(x.totalObligation != x.lastMonthObligation ? true : false))) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell4.Border = 0;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase(string.Format("{0:N}", x.totalObligation), GetFont(x.totalObligation != x.lastMonthObligation ? true : false))) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell5.Border = 0;
                    table.AddCell(cell5);
                }
                doc.Add(table);
            }
            
            table = new PdfPTable(5);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 2f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(string.IsNullOrEmpty(buisinessUnitCode) ? "JANAF - ukupno obustave:" : "Ukupno:", GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.monthlyFee), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.repayment), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(g.Currency(records.total.totalObligation), GetFont(true))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

     [WebMethod]
    public string Entry(int month, int year, Account.EntryTotal records) {
        try {
            PrintDoc pd = PreparePrintDoc(false);
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);

            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_CENTER;
            p.Add(new Paragraph(string.Format("Temeljnica za knjiženje br. {0}", month), GetFont(12, Font.BOLD)));
            p.Add(new Paragraph(string.Format("Knjižiti na dan {0}", g.SetDayMonthDate(g.GetLastDayInMonth(year, month), month)), GetFont()));
            doc.Add(p);

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 3f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("SADRŽAJ", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("KONTO", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("DUGUJE", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("POTRAŽUJE", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            foreach (Account.Recapitulation x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.note, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0:N}", x.account), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase(string.Format("{0:N}", x.output), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell3.Border = 0;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase(string.Format("{0:N}", x.input), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
            }
            doc.Add(table);

            table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 3f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 40, PaddingTop = 5, PaddingBottom = 15 });
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 40, PaddingTop = 5, PaddingBottom = 15, HorizontalAlignment = Element.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", g.Currency(records.total.output)), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 40, PaddingTop = 5, PaddingBottom = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", g.Currency(records.total.input)), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 40, PaddingTop = 5, PaddingBottom = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1f, 1f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase(string.Format(@"Kontrolirao:
       
                                                                     
Datum........................................"), GetFont(8))) { Border = PdfPCell.BOX, Padding = 2, MinimumHeight = 40, HorizontalAlignment = Element.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format(@"Knjižio:
 
                                                                           
Datum........................................"), GetFont(8))) { Border = PdfPCell.BOX, Padding = 2, MinimumHeight = 40, HorizontalAlignment = Element.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format(@"Rukovodilac računovodstva:
 
                                                                           
"), GetFont(8))) { Border = PdfPCell.BOX, Padding = 2, MinimumHeight = 40, HorizontalAlignment = Element.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format(@"Rukovodilac radne organizacije:
  
                                                                          
"), GetFont(8))) { Border = PdfPCell.BOX, Padding = 2, MinimumHeight = 40, HorizontalAlignment = Element.ALIGN_LEFT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Recapitulation(int month, Account.RecapYearlyTotal records, string title) {
        try {
            PrintDoc pd = PreparePrintDoc(Orientation());
            Document doc = pd.doc;
            doc.Open();
            AppendHeader(doc);
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_CENTER;
            p.Add(new Paragraph(string.Format("{0}", title), GetFont(12, Font.BOLD)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} god.", records.year), GetFont(10, Font.NORMAL))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("Konto: {0}", records.account), GetFont(10, Font.NORMAL))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            //p.Add(new Paragraph(string.Format("{0} god.", records.year), GetFont(10, Font.NORMAL)));
            //p.Add(new Paragraph(string.Format("Konto: {0}", records.account), GetFont(10, Font.NORMAL)));
            //doc.Add(p);

            table = new PdfPTable(records.type == g.giroaccount ? 5 : 4);
            table.WidthPercentage = 100f;
            if (records.type == g.giroaccount) {
                table.SetWidths(new float[] { 1f, 4f, 1f, 1f, 1f });
            } else {
                table.SetWidths(new float[] { 1f, 4f, 1f, 1f });
            }
            
            table.AddCell(new PdfPCell(new Phrase("Datum", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase("Sadržaj", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            if (records.type == g.giroaccount) {
                table.AddCell(new PdfPCell(new Phrase("Stanje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            table.AddCell(new PdfPCell(new Phrase("Duguje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Potražuje", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
          
            foreach (Account.RecapMonthlyTotal x in records.data) {
                PdfPCell cell1 = new PdfPCell(new Phrase(x.total.date, GetFont()));
                cell1.Border = 0;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase(string.Format("{0} {1}", x.month, x.total.note), GetFont()));
                cell2.Border = 0;
                table.AddCell(cell2);
                if (records.type == g.giroaccount) {
                    PdfPCell cell3 = new PdfPCell(new Phrase(x.total.accountBalance > 0 ? string.Format("{0:N}", x.total.accountBalance) : "", GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                    cell3.Border = 0;
                    table.AddCell(cell3);
                }
                PdfPCell cell4 = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.output), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4.Border = 0;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.input), GetFont())) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5.Border = 0;
                table.AddCell(cell5);

                /****** Accumulation *****/
                PdfPCell cell1_ = new PdfPCell(new Phrase("", GetFont()));
                cell1_.Border = 0;
                table.AddCell(cell1_);
                PdfPCell cell2_ = new PdfPCell(new Phrase("", GetFont()));
                cell2_.Border = 0;
                table.AddCell(cell2_);
                if (records.type == g.giroaccount) {
                    PdfPCell cell3_ = new PdfPCell(new Phrase("", GetFont()));
                    cell3_.Border = 0;
                    table.AddCell(cell3_);
                }
                PdfPCell cell4_ = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.outputAccumulation), GetFont(6, Font.ITALIC))) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell4_.Border = 0;
                table.AddCell(cell4_);
                PdfPCell cell5_ = new PdfPCell(new Phrase(string.Format("{0:N}", x.total.inputAccumulation), GetFont(6, Font.ITALIC))) { Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT };
                cell5_.Border = 0;
                table.AddCell(cell5_);
                /****** Accumulation *****/

            }
            doc.Add(table);

            table = new PdfPTable(records.type == g.giroaccount ? 5 : 4);
            table.WidthPercentage = 100f;
            if (records.type == g.giroaccount) {
                table.SetWidths(new float[] { 1f, 4f, 1f, 1f, 1f });
            } else {
                table.SetWidths(new float[] { 1f, 4f, 1f, 1f });
            }
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5 });
            if (records.type == g.giroaccount) {
                table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            table.AddCell(new PdfPCell(new Phrase("Ukupno:", GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", records.total.output), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N}", records.total.input), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);

            doc.Close();

            return JsonConvert.SerializeObject(pd.fileName, Formatting.Indented);
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

    private PrintDoc PreparePrintDoc(bool rotate) {
        PrintDoc x = new PrintDoc();
        GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
        Rectangle ps = PageSize.A4;
        Document doc = new Document();
        if (rotate) {
            doc = new Document(ps.Rotate());
        }
        string path = Server.MapPath("~/upload/pdf/temp/");
        g.DeleteFolder(path);
        g.CreateFolder(path);
        string fileName = Guid.NewGuid().ToString();
        string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
        PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
        x.doc = doc;
        x.fileName = fileName;
        return x;
    }

    private void AppendHeader(Document doc) {
        PdfPTable table = new PdfPTable(2);
        table.WidthPercentage = 100f;
        if (File.Exists(logoPath)) {
            Image logo = Image.GetInstance(logoPath);
            logo.Alignment = Image.ALIGN_RIGHT;
            logo.ScaleToFit(160f, 30f);
            logo.SpacingAfter = 15f;
            table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
            table.AddCell(new PdfPCell(new Phrase(s.Data().printSettings.headerInfo, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
        } else {
            table.AddCell(new PdfPCell(new Phrase(s.Data().printSettings.headerInfo, GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10, HorizontalAlignment = PdfPCell.ALIGN_LEFT });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
        }
        doc.Add(table);
        doc.Add(new Chunk(line));
    }

    private bool Orientation() {
        return s.Data().printSettings.orientation == "h" ? true : false;
    }

}
