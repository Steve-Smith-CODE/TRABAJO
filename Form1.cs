using System;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace TRABAJO
{
    public partial class form1 : Form
    {
        private bool columnasAgregadas = false;

        public form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboTipo.Items.Add("Contado");
            cboTipo.Items.Add("Tarjeta");
            cboTipo.SelectedIndex = 0;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
{   
    if (!columnasAgregadas)
    {
        dgvCurso.Columns.Add("Cantidad", "Cantidad");
        dgvCurso.Columns.Add("Descripción", "Descripción");
        dgvCurso.Columns.Add("PrecioUnitario", "Precio Unitario");
        dgvCurso.Columns.Add("Descuento", "Descuento");
        dgvCurso.Columns.Add("Importe", "Importe");
        dgvCurso.Columns.Add("Créditos", "Créditos");
        columnasAgregadas = true;
        dgvCurso.AllowUserToAddRows = false;
    }

    try
    {
        string tipo = cboTipo.Text;
        double subtotal = double.Parse(txtCantidad.Text) * double.Parse(txtPrecio.Text);
        double descuento = tipo.Equals("Contado") ? 0.05 * subtotal : 0;
        int indice_fila = dgvCurso.Rows.Add();
        DataGridViewRow fila = dgvCurso.Rows[indice_fila];
        fila.Cells["Cantidad"].Value = txtCantidad.Text;
        fila.Cells["Descripción"].Value = txtCurso.Text;
        fila.Cells["PrecioUnitario"].Value = txtPrecio.Text;
        fila.Cells["Descuento"].Value = descuento.ToString("0.00");
        fila.Cells["Importe"].Value = (subtotal - descuento).ToString("0.00");
        fila.Cells["Créditos"].Value = txtCredito.Text;
    }
    catch (Exception ex)
    {
        MessageBox.Show("Error al agregar fila: " + ex.Message);
    }
}

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = $"Reporte_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}" + ".pdf";

            string PaginaHTML_Texto = Properties.Resources.plantilla;

            if (string.IsNullOrEmpty(txtNombres.Text) || string.IsNullOrEmpty(txtDocumento.Text))
            {
                MessageBox.Show("Por favor complete todos los campos.");
                return;
            }

            PaginaHTML_Texto = PaginaHTML_Texto.Replace("@ESTUDIANTE", txtNombres.Text);
            PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DOCUMENTO", txtDocumento.Text);
            PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FECHA", DateTime.Now.ToString("dd/MM/yyyy"));

            string filas = string.Empty;
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCurso.Rows)
            {
                if (row.Cells["Cantidad"].Value != null &&
                    row.Cells["Descripción"].Value != null &&
                    row.Cells["PrecioUnitario"].Value != null &&
                    row.Cells["Importe"].Value != null)
                {
                    filas += "<tr>";
                    filas += "<td>" + row.Cells["Cantidad"].Value.ToString() + "</td>";
                    filas += "<td>" + row.Cells["Descripción"].Value.ToString() + "</td>";
                    filas += "<td>" + row.Cells["PrecioUnitario"].Value.ToString() + "</td>";
                    filas += "<td>" + row.Cells["Importe"].Value.ToString() + "</td>";
                    filas += "<td>" + row.Cells["Créditos"].Value.ToString() + "</td>";
                    filas += "</tr>";
                    total += decimal.Parse(row.Cells["Importe"].Value.ToString());
                }
                else
                {
                    MessageBox.Show("Asegúrese de que todas las filas tengan valores completos.");
                    return;
                }
            }
            PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FILAS", filas);
            PaginaHTML_Texto = PaginaHTML_Texto.Replace("@TOTAL_MATRICULA", total.ToString("0.00"));

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                string filePath = savefile.FileName;
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    pdfDoc.Add(new Phrase(""));

                    try
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(Properties.Resources.images, System.Drawing.Imaging.ImageFormat.Png);
                        img.ScaleToFit(80, 80);
                        img.Alignment = iTextSharp.text.Image.UNDERLYING;
                        img.SetAbsolutePosition(pdfDoc.LeftMargin, pdfDoc.Top - 60);
                        pdfDoc.Add(img);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al agregar la imagen: " + ex.Message);
                    }

                    using (StringReader sr = new StringReader(PaginaHTML_Texto))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    }

                    pdfDoc.Close();
                    stream.Close();
                }
            }
        }




        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtCantidad.Text = "";
            txtCurso.Text = "";
            txtPrecio.Text = "";
            cboTipo.SelectedIndex = 0;
        }
    }
}
