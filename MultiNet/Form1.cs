using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiNet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtEttn.Text = Guid.NewGuid().ToString().ToUpper();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();

            var path = folder.SelectedPath;
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Geçerli bir dosya seçin!");
            }
            else
            {
                var result = 0;
                try
                {
                    using (var conn = new SQLiteConnection("Data Source=MultiNet.db;"))
                    {

                        var cmd = new SQLiteCommand("INSERT INTO FaturaNo (BelgeNo,CrtDate) values (@belgeno,@CrtDate);", conn);
                        cmd.Parameters.AddWithValue("@belgeno", txtBelgeNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@CrtDate", DateTime.Now);
                        conn.Open();

                        result = cmd.ExecuteNonQuery();
                    }
                    if (result>0)
                    {
                        // Template dosyası okunuyor...
                        PdfReader pdfReader = new PdfReader("2.pdf");
                        var fileName = path + "\\Fatura_" + txtAdSoyad.Text.Replace(' ', '_') + ".pdf";
                        PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(fileName, FileMode.Create));
                        AcroFields pdfFormFields = pdfStamper.AcroFields;


                        // Türkçe karakter için font seçiliyor...
                        BaseFont ARIAL = BaseFont.CreateFont("C:\\windows\\fonts\\arial.ttf", "windows-1254", true);
                        pdfFormFields.AddSubstitutionFont(ARIAL);
                        foreach (var f in pdfReader.AcroFields.Fields)
                        {
                            pdfFormFields.SetFieldProperty(f.Key.ToString(), "textsize", (float)7, null);
                        }

                        pdfFormFields.SetField("AdSoyad", txtAdSoyad.Text.Trim());
                        pdfFormFields.SetField("Sehir", txtSehir.Text.Trim());
                        pdfFormFields.SetField("TCKimlikno", txtTc.Text.Trim());
                        pdfFormFields.SetField("BelgeNo", txtBelgeNo.Text.Trim());
                        pdfFormFields.SetField("FaturaTarihi", txtFaturaTarihi.Text.Trim());
                        pdfFormFields.SetField("DuzenlemeTarihi", txtDuzenlemeTarihi.Text.Trim());
                        pdfFormFields.SetField("DuzenlemeZamani", txtDuzenlemeZamani.Text.Trim());
                        pdfFormFields.SetField("Ettn", txtEttn.Text.Trim());

                        pdfFormFields.SetField("Kod", txtMalKod.Text.Trim());
                        pdfFormFields.SetField("MalAd", txtMalAd.Text.Trim());
                        pdfFormFields.SetField("MalAciklama", txtMalAciklama.Text.Trim());
                        pdfFormFields.SetField("Miktar", txtMalMiktar.Text.Trim());
                        pdfFormFields.SetField("BirimFiyat", txtMalBirimFiyat.Text.Trim());
                        pdfFormFields.SetField("KdvOrani", txtMalKdvOrani.Text.Trim());
                        pdfFormFields.SetField("KdvTutar", txtMalKdvTutar.Text.Trim());
                        pdfFormFields.SetField("HizmetTutar", txtMalTutar.Text.Trim());

                        pdfFormFields.SetField("ToplamTutar", txtToplamTutar.Text.Trim());
                        pdfFormFields.SetField("HesaplananKdv", txtHesaplananKdv.Text.Trim());
                        pdfFormFields.SetField("VergilerDahilToplam", txtToplamVergi.Text.Trim());
                        pdfFormFields.SetField("OdenecekTutar", txtOdenecekTutar.Text.Trim());

                        pdfFormFields.SetField("OdemeKosulu", txtOdemeKosulu.Text.Trim());
                        pdfFormFields.SetField("GenelAciklama", txtGenelAciklama.Text.Trim());
                        pdfFormFields.SetField("KartNo", txtKartNo.Text.Trim());

                        pdfStamper.Close();
                        MessageBox.Show("Belge Kaydedildi");
                    }
                    else
                    {
                        MessageBox.Show("Belge No Hatalı");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Belge No Geçerli değil");
                }

            }
            

        }

        private void txtMalMiktar_KeyDown(object sender, KeyEventArgs e)
        {
            decimal miktar = 0;
            decimal birimf = 0;
            decimal kdvoran = 0;
            try
            {
                miktar = Convert.ToDecimal(txtMalMiktar.Text);
            }
            catch
            {
                miktar = 0;
            }

            try
            {
                birimf = Convert.ToDecimal(txtMalBirimFiyat.Text);
            }
            catch
            {
                birimf = 0;
            }

            try
            {
                kdvoran = Convert.ToDecimal(txtMalKdvOrani.Text);
            }
            catch
            {
                kdvoran = 0;
            }

            txtMalTutar.Text =txtToplamTutar.Text= (miktar * birimf).ToString("C");
            txtMalKdvTutar.Text = txtHesaplananKdv.Text = ((miktar * birimf) * kdvoran).ToString("C");

        }
    }
}
