using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace AjuNice
{
    public partial class Login : Form
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


        public Login()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Code for when username text changes
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Code for panel styling
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Code for clicking the logo/image
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Get the user input
            string user = txtUsername.Text;
            string pass = txtPassword.Text;

            // 2. Use your public connection class
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                try
                {
                    conn.Open();
                    // We check for the username, password, and if the account is 'Active'
                    string query = "SELECT fname, lname FROM Employees WHERE username = @user AND password = @pass AND status = 'Active'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        MessageBox.Show("Login Successful! Aju Nice!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 3. Hide login and show Dashboard
                        this.Hide();
                        Dashboard dash = new Dashboard();
                        dash.Show();
                    }
                    else
                    {
                        MessageBox.Show("Invalid Username or Password, or Account is Inactive.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string inputPassword = txtPassword.Text;

            // 2. Hash it using the same method
            string hashedInput = HashPassword(inputPassword);

            // 3. Compare it in your SQL
            string query = "SELECT * FROM employees WHERE username=@u AND password=@p AND status='Active'";
            // ... add parameters (@u for username, @p for hashedInput)
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            passRecovery recovery = new passRecovery();

            // This makes the recovery form show up where the login form was
            recovery.StartPosition = FormStartPosition.Manual;
            recovery.Location = this.Location;

            this.Hide(); // Hide the login form
            recovery.ShowDialog(); // Show recovery
            this.Show(); // Show login again once recovery is closed

        }

        // Note: I removed pictureBox2 and label1 because they 
        // weren't in the Designer code you shared.
    }
}