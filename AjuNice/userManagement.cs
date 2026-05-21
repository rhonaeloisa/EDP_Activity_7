using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;


namespace AjuNice
{

    public partial class userManagement : Form
    {
        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public void LoadAccountList()
        {
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                // Adjust column names if they differ in your MySQL table
                string query = "SELECT employee_id, fname, lname, hire_date, username, password, status FROM Employees";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvAccounts.DataSource = dt;
            }
        }


        public userManagement()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = "SELECT * FROM Employees WHERE fname LIKE @s OR lname LIKE @s OR username LIKE @s";
                MySqlDataAdapter adp = new MySqlDataAdapter(query, conn);
                adp.SelectCommand.Parameters.AddWithValue("@s", "%" + txtSearch.Text + "%");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                dgvAccounts.DataSource = dt;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void dgvAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvAccounts.Rows[e.RowIndex];
                txtEmployeeID.Text = row.Cells["employee_id"].Value.ToString();
                txtFname.Text = row.Cells["fname"].Value.ToString();
                txtLname.Text = row.Cells["lname"].Value.ToString();
                txtHireDate.Text = row.Cells["hire_date"].Value.ToString();
                txtPosition.Text = row.Cells["position_name"].Value.ToString();
                txtUsername.Text = row.Cells["username"].Value.ToString();
                txtPassword.Text = row.Cells["password"].Value.ToString();
                cmbStatus.Text = row.Cells["status"].Value.ToString();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Included employee_id in the INSERT statement
                    string query = "INSERT INTO employees (employee_id, fname, lname, position_id, hire_date, username, password, status) " +
                                   "VALUES (@id, @f, @l, @p, @h, @u, @pass, @s)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtEmployeeID.Text); // Manually adding ID
                    cmd.Parameters.AddWithValue("@f", txtFname.Text);
                    cmd.Parameters.AddWithValue("@l", txtLname.Text);
                    cmd.Parameters.AddWithValue("@p", int.Parse(txtPosition.Text));
                    cmd.Parameters.AddWithValue("@h", txtHireDate.Text);
                    cmd.Parameters.AddWithValue("@u", txtUsername.Text);

                    // Secure the password before saving
                    cmd.Parameters.AddWithValue("@pass", HashPassword(txtPassword.Text));

                    cmd.Parameters.AddWithValue("@s", cmbStatus.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("New Staff Added! Aju Nice!");
                    LoadAccountList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Check if ID already exists or Position is a number. \n" + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow == null) return;

            string id = dgvAccounts.CurrentRow.Cells["employee_id"].Value.ToString();

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "UPDATE employees SET fname=@f, lname=@l, position_id=@p, hire_date=@h, username=@u, password=@pass, status=@s " +
                               "WHERE employee_id=@id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@f", txtFname.Text);
                cmd.Parameters.AddWithValue("@l", txtLname.Text);
                cmd.Parameters.AddWithValue("@p", int.Parse(txtPosition.Text));
                cmd.Parameters.AddWithValue("@h", txtHireDate.Text);
                cmd.Parameters.AddWithValue("@u", txtUsername.Text);
                cmd.Parameters.AddWithValue("@pass", HashPassword(txtPassword.Text)); // Hash on update too
                cmd.Parameters.AddWithValue("@s", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Employee " + id + " updated successfully!");
                LoadAccountList();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow == null) return;
            string id = dgvAccounts.CurrentRow.Cells["employee_id"].Value.ToString();

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                // Simply toggles the status based on what is in the ComboBox
                string query = "UPDATE Employees SET status=@s WHERE employee_id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@s", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Account status is now: " + cmbStatus.Text);
                LoadAccountList();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtEmployeeID.Clear();
            txtFname.Clear();
            txtLname.Clear();
            txtHireDate.Clear();
            txtPosition.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            cmbStatus.SelectedIndex = -1;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAccountList();

            // Optional: Show a small message or clear the search box
            txtSearch.Clear();
            MessageBox.Show("List updated! Aju Nice!", "System");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Dashboard dash = new Dashboard();

            // 2. Keep the window in the same spot on the screen
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;

            // 3. Close or Hide this form and show the Dashboard
            this.Hide();
            dash.Show();
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 1. Create an instance of your Reports form
            Reports rep = new Reports();

            // 2. Position it exactly where the Dashboard currently is to prevent screen jumping
            rep.StartPosition = FormStartPosition.Manual;
            rep.Location = this.Location;

            // 3. Hide the Dashboard and show the Reports form
            this.Hide();
            rep.Show();
        }
    }
}
