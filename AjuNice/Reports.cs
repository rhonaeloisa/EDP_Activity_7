using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// 1. ADDED EXCEL INTEROP ALIAS HERE SO THE SYSTEM RECOGNIZES EXCEL OBJECTS
using Excel = Microsoft.Office.Interop.Excel;

namespace AjuNice
{
    public partial class Reports : Form
    {
        public Reports()
        {
            InitializeComponent();

            // 2. POPULATING THE CATEGORY DROPDOWN ON FORM INITIALIZATION
            cmbReportType.Items.Clear();
            cmbReportType.Items.AddRange(new string[] { "Sales Transactions", "Inventory Stock Status", "Supplier Orders Log" });
            cmbReportType.SelectedIndex = 0;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel18_Paint(object sender, PaintEventArgs e)
        {

        }

        // 1. GENERATE FILTERED DATA INTO THE GRID VIEW
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string query = "";

            // Dynamic SQL using your schema structure and date range logic
            if (cmbReportType.Text == "Sales Transactions")
            {
                // Joins back to your core orders table to filter by order_date
                query = "SELECT s.order_id AS 'Order ID', s.customer AS 'Customer Name', s.menu_name AS 'Item', s.quantity AS 'Qty Ordered', s.total_price AS 'Total Sales (PHP)' " +
                        "FROM salesreport s JOIN orders o ON s.order_id = o.order_id " +
                        "WHERE o.order_date BETWEEN @start AND @end";
            }
            else if (cmbReportType.Text == "Inventory Stock Status")
            {
                // Inventory status represents a current snapshot, so date filtering is optional here
                query = "SELECT menu_name AS 'Item Name', stock_quantity AS 'Available Stock' FROM inventoryreport";
            }
            else if (cmbReportType.Text == "Supplier Orders Log")
            {
                // Uses the supply_date column directly from your supplyorders table
                query = "SELECT s.supply_order_id AS 'Supply Order ID', su.supplier_name AS 'Supplier Inc.', s.inventory_id AS 'Inv ID', s.quantity_supplied AS 'Qty Supplied', s.supply_date AS 'Date Received' " +
                        "FROM supplyorders s JOIN suppliers su ON s.supplier_id = su.supplier_id " +
                        "WHERE s.supply_date BETWEEN @start AND @end";
            }

            try
            {
                using (MySqlConnection conn = DatabaseConnection.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Reading from your textboxes directly (.Text)
                    // Note: Ensure your TextBoxes are named dtpStart and dtpEnd in your Properties panel (F4)
                    cmd.Parameters.AddWithValue("@start", dtpStart.Text);
                    cmd.Parameters.AddWithValue("@end", dtpEnd.Text);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvReports.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Alert");
            }
        }

        // 2. EXPORT TO DUAL-SHEET EXCEL TEMPLATE WITH CHART & SIGNATURE PLACEMENTS
        

        // 3. ADDED NAVIGATION SYSTEM EVENT HOOK TO LOOP BACK TO THE HOME SCREEN
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Dashboard dash = new Dashboard();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            this.Hide();
            dash.Show();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 1. Create an instance of the userManagement form
            userManagement manage = new userManagement();

            // 2. Keep the window in the exact same desktop bounds to prevent screen jumping
            manage.StartPosition = FormStartPosition.Manual;
            manage.Location = this.Location;

            // 3. Hide the Reports form and show the User Management form
            this.Hide();
            manage.Show();
        }

        private void btnGenerate_Click_1(object sender, EventArgs e)
        {
            string query = "";

            if (cmbReportType.Text == "Sales Transactions")
            {
                // Explicitly joining your view to the orders table and forcing date string conversion
                query = "SELECT s.order_id AS 'Order ID', s.customer AS 'Customer Name', s.menu_name AS 'Item', s.quantity AS 'Qty Ordered', s.total_price AS 'Total Sales (PHP)' " +
                        "FROM salesreport s " +
                        "JOIN orders o ON s.order_id = o.order_id " +
                        "WHERE DATE(o.order_date) BETWEEN STR_TO_DATE(@start, '%Y-%m-%d') AND STR_TO_DATE(@end, '%Y-%m-%d')";
            }
            else if (cmbReportType.Text == "Inventory Stock Status")
            {
                query = "SELECT menu_name AS 'Item Name', stock_quantity AS 'Available Stock' FROM inventoryreport";
            }
            else if (cmbReportType.Text == "Supplier Orders Log")
            {
                query = "SELECT s.supply_order_id AS 'Supply Order ID', su.supplier_name AS 'Supplier Inc.', s.inventory_id AS 'Inv ID', s.quantity_supplied AS 'Qty Supplied', s.supply_date AS 'Date Received' " +
                        "FROM supplyorders s " +
                        "JOIN suppliers su ON s.supplier_id = su.supplier_id " +
                        "WHERE DATE(s.supply_date) BETWEEN STR_TO_DATE(@start, '%Y-%m-%d') AND STR_TO_DATE(@end, '%Y-%m-%d')";
            }

            try
            {
                using (MySqlConnection conn = DatabaseConnection.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // .Trim() removes any accidental spaces you might type
                    cmd.Parameters.AddWithValue("@start", dtpStart.Text.Trim());
                    cmd.Parameters.AddWithValue("@end", dtpEnd.Text.Trim());

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvReports.DataSource = dt;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No records found for this date range. Double-check your database connections!", "System Info");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Alert");
            }
        }

        private void btnExportExcel_Click_1(object sender, EventArgs e)
        {
            if (dgvReports.Rows.Count == 0)
            {
                MessageBox.Show("Please generate data into the grid view before exporting!", "System Alert");
                return;
            }

            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = true;

            Excel.Workbook workbook = excelApp.Workbooks.Add(System.Reflection.Missing.Value);

            // --- SHEET 1: DATA TABLE, LOGO, & SIGNATURE BLOCKS ---
            // --- SHEET 1: DATA TABLE, LOGO, & SIGNATURE BLOCKS ---
            Excel.Worksheet sheet1 = (Excel.Worksheet)workbook.Sheets[1];
            sheet1.Name = "Audit Log";

            // 1. DYNAMICALLY TARGET THE ACTIVE PROJECT DIRECTORY FOR LOGO
            string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");

            try
            {
                if (System.IO.File.Exists(logoPath))
                {
                    // Keeps the logo anchored at cell position (Left: 25, Top: 15)
                    sheet1.Shapes.AddPicture(logoPath,
                                             Microsoft.Office.Core.MsoTriState.msoFalse,
                                             Microsoft.Office.Core.MsoTriState.msoTrue,
                                             25, 15, 50, 50);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logo mapping skipped: " + ex.Message, "System Note");
            }

            // 2. MOVED HEADER TEXT TO COLUMN C (Shifts left a little bit for a tighter fit)
            sheet1.Cells[2, 3] = "AJU NICE FAST FOOD CORP.";
            sheet1.get_Range("C2", "G2").Merge(); // Merges from C to G
            sheet1.Cells[2, 3].Font.Size = 16;
            sheet1.Cells[2, 3].Font.Bold = true;
            sheet1.Cells[2, 3].Font.Color = ColorTranslator.ToOle(Color.Tomato);

            // 3. MOVED CATEGORY SUBTITLE TO COLUMN C & EXTENDED MERGE TO COLUMN I
            sheet1.Cells[3, 3] = $"Transaction Category: {cmbReportType.Text} ({dtpStart.Text} to {dtpEnd.Text})";
            sheet1.get_Range("C3", "I3").Merge(); // Shifting sheet1.get_Range here fixes your old crash entirely!
            sheet1.get_Range("C3", "I3").Font.Italic = true;

            // 3. Set your italic style on the merged block range smoothly
            sheet1.get_Range("D3", "H3").Font.Italic = true;

            int startRow = 6;
            int startCol = 2; // Column B

            // Columns Header Matrix
            for (int col = 0; col < dgvReports.Columns.Count; col++)
            {
                Excel.Range headerCell = sheet1.Cells[startRow, startCol + col];
                headerCell.Value = dgvReports.Columns[col].HeaderText;
                headerCell.Font.Bold = true;
                headerCell.Interior.Color = ColorTranslator.ToOle(Color.MistyRose);
                headerCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            }

            // Row Population
            for (int row = 0; row < dgvReports.Rows.Count; row++)
            {
                for (int col = 0; col < dgvReports.Columns.Count; col++)
                {
                    if (dgvReports.Rows[row].Cells[col].Value != null)
                    {
                        Excel.Range dataCell = sheet1.Cells[startRow + 1 + row, startCol + col];
                        dataCell.Value = dgvReports.Rows[row].Cells[col].Value.ToString();
                        dataCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    }
                }
            }

            sheet1.get_Range("B6", (char)('B' + dgvReports.Columns.Count - 1) + (startRow + dgvReports.Rows.Count).ToString()).Columns.AutoFit();

            // Signature Placeholders
            int signatureRow = startRow + dgvReports.Rows.Count + 4;
            sheet1.Cells[signatureRow, 2] = "Prepared By:";
            sheet1.Cells[signatureRow + 2, 2] = "_________________________";
            sheet1.Cells[signatureRow + 3, 2] = "System Administrator Account";
            sheet1.Cells[signatureRow + 3, 2].Font.Italic = true;

            sheet1.Cells[signatureRow, 5] = "Reviewed & Verified By:";
            sheet1.Cells[signatureRow + 2, 5] = "_________________________";
            sheet1.Cells[signatureRow + 3, 5] = "Operations Management Office";
            sheet1.Cells[signatureRow + 3, 5].Font.Italic = true;

            // --- SHEET 2: THE DYNAMIC CHART ---
            Excel.Worksheet sheet2 = (Excel.Worksheet)workbook.Sheets.Add(Type.Missing, sheet1, Type.Missing, Type.Missing);
            sheet2.Name = "Visual Analytics";

            Excel.ChartObjects chartCols = (Excel.ChartObjects)sheet2.ChartObjects(Type.Missing);
            Excel.ChartObject chartObj = chartCols.Add(50, 50, 650, 400);
            Excel.Chart analyticalChart = chartObj.Chart;

            int endRowIndex = startRow + dgvReports.Rows.Count;
            Excel.Range sourceRange;

            if (cmbReportType.Text == "Inventory Stock Status")
            {
                // Item Name (Col B) vs Available Stock (Col C)
                sourceRange = sheet1.get_Range("B6", "C" + endRowIndex);
                analyticalChart.SetSourceData(sourceRange, Excel.XlRowCol.xlColumns);
                analyticalChart.ChartType = Excel.XlChartType.xlColumnClustered;
            }
            else if (cmbReportType.Text == "Sales Transactions")
            {
                // Item Name (Col D) vs Total Price (Col F)
                Excel.Range itemsRange = sheet1.get_Range("D6", "D" + endRowIndex);
                Excel.Range priceRange = sheet1.get_Range("F6", "F" + endRowIndex);
                sourceRange = excelApp.Union(itemsRange, priceRange);
                analyticalChart.SetSourceData(sourceRange, Excel.XlRowCol.xlColumns);
                analyticalChart.ChartType = Excel.XlChartType.xlBarClustered;
            }
            else
            {
                sourceRange = sheet1.get_Range("B6", (char)('B' + dgvReports.Columns.Count - 1) + endRowIndex.ToString());
                analyticalChart.SetSourceData(sourceRange, Excel.XlRowCol.xlColumns);
                analyticalChart.ChartType = Excel.XlChartType.xlLine;
            }

            analyticalChart.HasTitle = true;
            analyticalChart.ChartTitle.Text = cmbReportType.Text + " Data Trend Summary";

            MessageBox.Show("Business Spreadsheet generated successfully with integrated analytics! Aju Nice!", "Export Done");
        }
    
    }
    
}