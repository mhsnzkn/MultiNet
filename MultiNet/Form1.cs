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
        const string ConnStr = "Data Source=MultiNet.db;";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtEttn.Text = Guid.NewGuid().ToString().ToUpper();
            txtSehir.Text = "Ankara";
            txtTc.Text = "111111111111";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();

            var path = folder.SelectedPath;
            if (string.IsNullOrEmpty(txtBelgeNo.Text))
            {
                MessageBox.Show("Belge No girmediniz!");
            }
            else if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Geçerli bir dosya yolu seçin!");
            }
            else
            {
                var result = 0;
                try
                {
                    using (var conn = new SQLiteConnection(ConnStr))
                    {
                        var checkCmd = new SQLiteCommand("select count(*) from FaturaNo where BelgeNo=@belgeno",conn);
                        checkCmd.Parameters.AddWithValue("@belgeno", txtBelgeNo.Text.Trim());
                        conn.Open();
                        var checkResult = (long)checkCmd.ExecuteScalar();
                        if (checkResult == 0)
                        {
                            var cmd = new SQLiteCommand("INSERT INTO FaturaNo (BelgeNo,CrtDate) values (@belgeno,@CrtDate);", conn);
                            cmd.Parameters.AddWithValue("@belgeno", txtBelgeNo.Text.Trim());
                            cmd.Parameters.AddWithValue("@CrtDate", DateTime.Now);

                            result = cmd.ExecuteNonQuery();

                            if (result > 0)
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
                                pdfFormFields.SetField("Miktar", txtMalMiktar.Text.Trim() + " ADET");
                                pdfFormFields.SetField("BirimFiyat", txtMalBirimFiyat.Text.Trim() + " TL");
                                pdfFormFields.SetField("KdvOrani", "% " + txtMalKdvOrani.Text.Trim());
                                pdfFormFields.SetField("KdvTutar", txtMalKdvTutar.Text.Trim() + " TL");
                                pdfFormFields.SetField("HizmetTutar", txtMalTutar.Text.Trim() + " TL");

                                pdfFormFields.SetField("ToplamTutar", txtToplamTutar.Text.Trim() + " TL");
                                pdfFormFields.SetField("HesaplananKdv", txtHesaplananKdv.Text.Trim() + " TL");
                                pdfFormFields.SetField("VergilerDahilToplam", txtToplamVergi.Text.Trim() + " TL");
                                pdfFormFields.SetField("OdenecekTutar", txtOdenecekTutar.Text.Trim() + " TL");

                                pdfFormFields.SetField("OdemeKosulu", txtOdemeKosulu.Text.Trim());
                                pdfFormFields.SetField("GenelAciklama", txtGenelAciklama.Text.Trim());
                                pdfFormFields.SetField("KartNo", txtKartNo.Text.Trim());

                                pdfStamper.Close();
                                MessageBox.Show("Belge Kaydedildi");
                            }
                            else
                            {
                                MessageBox.Show("Belge Kaydedilemedi!", "UYARI");
                            }

                        }
                        else
                            MessageBox.Show("Belge No Daha önce kullanılmış");


                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata Oldu: "+ex.ToString());
                }

            }


        }

        private void MenuItemGiris_Click(object sender, EventArgs e)
        {
            panelSorgu.Visible = false;
        }

        private void MenuItemBelgeSearch_Click(object sender, EventArgs e)
        {
            panelSorgu.Visible = true;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

            using (var conn = new SQLiteConnection("Data Source=MultiNet.db;"))
            {

                var cmd = new SQLiteCommand("SELECT Id as Sıra, BelgeNo as BelgeNo, CrtDate as Tarih FROM FaturaNo", conn);
                if (!string.IsNullOrEmpty(txtBelgeNoSorgu.Text.Trim()))
                {
                    cmd.CommandText += " WHERE BelgeNo like @belgeno";
                    cmd.Parameters.AddWithValue("@belgeno", "%"+txtBelgeNoSorgu.Text.Trim()+"%");
                }
                conn.Open();

                var dt = new DataTable();
                SQLiteDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                dgvBelgeNoArama.DataSource = dt;
            }
        }

        private void txtMalMiktar_KeyUp(object sender, KeyEventArgs e)
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
                kdvoran = Convert.ToDecimal(txtMalKdvOrani.Text) / 100;
            }
            catch
            {
                kdvoran = 0;
            }

            var toplamTutar = miktar * birimf;
            txtMalTutar.Text = txtToplamTutar.Text = (miktar * birimf).ToString("N");
            var kdvTutar = toplamTutar * kdvoran;
            txtMalKdvTutar.Text = txtHesaplananKdv.Text = kdvTutar.ToString("N");
            txtToplamVergi.Text = txtOdenecekTutar.Text = (toplamTutar + kdvTutar).ToString("N");
            txtGenelAciklama.Text = YaziyaCevir(toplamTutar + kdvTutar);
        }


        public static string YaziyaCevir(decimal tutar)
        {
            var yazi = "";
            try
            {

            var sTutar = tutar.ToString("F2").Replace('.', ','); // Replace('.',',') ondalık ayracının . olma durumu için            
            var lira = sTutar.Substring(0, sTutar.IndexOf(',')); //tutarın tam kısmı
            var kurus = sTutar.Substring(sTutar.IndexOf(',') + 1, 2);

            string[] birler = { "", "BİR", "İKİ", "ÜÇ", "DÖRT", "BEŞ", "ALTI", "YEDİ", "SEKİZ", "DOKUZ" };
            string[] onlar = { "", "ON", "YİRMİ", "OTUZ", "KIRK", "ELLİ", "ALTMIŞ", "YETMİŞ", "SEKSEN", "DOKSAN" };
            string[] binler = { "KATRİLYON", "TRİLYON", "MİLYAR", "MİLYON", "BİN", "" }; //KATRİLYON'un önüne ekleme yapılarak artırabilir.

            var grupSayisi = 6;
            //sayıdaki 3'lü grup sayısı. katrilyon içi 6. (1.234,00 daki grup sayısı 2'dir.)
            //KATRİLYON'un başına ekleyeceğiniz her değer için grup sayısını artırınız.

            lira = lira.PadLeft(grupSayisi * 3, '0'); //sayının soluna '0' eklenerek sayı 'grup sayısı x 3' basakmaklı yapılıyor.          

            for (int i = 0; i < grupSayisi * 3; i += 3) //sayı 3'erli gruplar halinde ele alınıyor.
            {
                var grupDegeri = "";

                if (lira.Substring(i, 1) != "0")
                    grupDegeri += birler[Convert.ToInt32(lira.Substring(i, 1))] + "YÜZ"; //yüzler                

                if (grupDegeri == "BİRYÜZ") //biryüz düzeltiliyor.
                    grupDegeri = "YÜZ";

                grupDegeri += onlar[Convert.ToInt32(lira.Substring(i + 1, 1))]; //onlar

                grupDegeri += birler[Convert.ToInt32(lira.Substring(i + 2, 1))]; //birler                

                if (grupDegeri != "") //binler
                    grupDegeri += binler[i / 3];

                if (grupDegeri == "BİRBİN") //birbin düzeltiliyor.
                    grupDegeri = "BİN";

                yazi += grupDegeri;
            }

            if (yazi != "")
                yazi += " TL ";

            int yaziUzunlugu = yazi.Length;

            if (kurus.Substring(0, 1) != "0") //kuruş onlar
                yazi += onlar[Convert.ToInt32(kurus.Substring(0, 1))];

            if (kurus.Substring(1, 1) != "0") //kuruş birler
                yazi += birler[Convert.ToInt32(kurus.Substring(1, 1))];

            if (yazi.Length > yaziUzunlugu)
                yazi += " Kr.";
            else
                yazi += "SIFIR Kr.";

            }
            catch 
            {

            }
            return yazi;
        }

        private void dgvBelgeNoArama_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvBelgeNoArama.SelectedCells.Count > 0)
            {
                int selectedrowindex = dgvBelgeNoArama.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dgvBelgeNoArama.Rows[selectedrowindex];
                lblBelgeNoSearch.Text = Convert.ToString(selectedRow.Cells["BelgeNo"].Value);

                btnSil.Enabled = true;
            }


        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (lblBelgeNoSearch.Text.Length>0)
            {
                DialogResult dialogResult = MessageBox.Show("Seçtiğiniz " + lblBelgeNoSearch.Text + " Nolu Belge Silinecektir. Silmek istediğinize emin misiniz?", "Silme İşlemi", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    var result = 0;
                    using (var conn = new SQLiteConnection(ConnStr))
                    {
                        var cmd = new SQLiteCommand("Delete from FaturaNo where BelgeNo=@belgeno", conn);
                        cmd.Parameters.AddWithValue("@belgeno", lblBelgeNoSearch.Text.Trim());
                        conn.Open();
                        result=cmd.ExecuteNonQuery();
                    }
                    if (result>0)
                    {
                        MessageBox.Show("Başarıyla silindi");
                        btnSearch.PerformClick();
                    }
                    else
                        MessageBox.Show("Silme işlemi başarısız");
                }
                else if (dialogResult == DialogResult.No)
                {

                }
            }
            else
            {
                MessageBox.Show("Geçerli bir Belge No Seçmediniz!");
            }
            
        }
    }
}
