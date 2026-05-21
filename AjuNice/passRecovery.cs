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

namespace AjuNice
{
    public partial class passRecovery : Form
    {
        public passRecovery()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // Logic for when the group box is entered (usually empty)
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Logic for when the instruction label is clicked
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (txtNewPass.Text == txtConfirmPass.Text)
            {
                using (MySqlConnection conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Employees SET password = @pass WHERE employee_id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@pass", txtNewPass.Text);
                    cmd.Parameters.AddWithValue("@id", txtEmployeeID.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Password updated successfully! Aju Nice!");
                    this.Close(); // Close recovery form and go back to Login
                }
            }
            else
            {
                MessageBox.Show("Passwords do not match!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                // Check if ID and Last Name match
                string query = "SELECT * FROM Employees WHERE employee_id = @id AND lname = @lname";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtEmployeeID.Text);
                cmd.Parameters.AddWithValue("@lname", txtLastName.Text);

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    // IDENTITY VERIFIED!
                    MessageBox.Show("Identity confirmed. You can now reset your password.");

                    // STEP 2 REVEALED
                    pnlStep2.Visible = true;

                    // Lock Step 1 so they don't change the ID while resetting
                    txtEmployeeID.Enabled = false;
                    txtLastName.Enabled = false;
                    btnVerify.Enabled = false;
                }
                else
                {
                    MessageBox.Show("ID or Last Name does not match our records.");
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close(); // Closes recovery
                          // In your Login form, you'd handle the 'FormClosed' event to show it again, 
                          // or just call Login again.
            Login login = new Login();
            login.Show();
        }
    }
}