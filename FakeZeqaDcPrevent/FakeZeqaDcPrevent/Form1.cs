using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FakeZeqaDcPrevent
{
    public partial class Form1 : Form
    {
        // Importing the necessary Windows API functions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Hook types and delegate for mouse events
        private const int WH_MOUSE_LL = 14;  // Low-level mouse hook
        private delegate IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr hookHandle = IntPtr.Zero;
        private MouseProc mouseProcDelegate;

        // NotifyIcon for system tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // Variable to track time between clicks
        private DateTime lastClickTime;

        public Form1()
        {
            InitializeComponent();

            // Initialize the first TrackBar properties
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 100;
            trackBar1.TickFrequency = 10;  // Set the TickFrequency to 10 to show 10 ticks
            trackBar1.Value = 0;
            textBox1.Text = trackBar1.Value.ToString();

            // Add the event handler for the first TrackBar's scroll
            trackBar1.Scroll += TrackBar1_Scroll;

            // Add the event handler for the first reset button's click event
            Button1.Click += Button1_Click;

            // Initialize the second TrackBar properties
            trackBar2.Minimum = 0;
            trackBar2.Maximum = 100;
            trackBar2.TickFrequency = 10;  // Set the TickFrequency to 10 to show 10 ticks
            trackBar2.Value = 0;
            textBox2.Text = trackBar2.Value.ToString();

            // Add the event handler for the second TrackBar's scroll
            trackBar2.Scroll += TrackBar2_Scroll;

            // Add the event handler for the second reset button's click event
            button2.Click += Button2_Click;

            // Add the event handler for checkBox2 CheckedChanged event
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;

            // Add the event handler for checkBox3 CheckedChanged event (lock functionality)
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;

            // Initialize the global mouse hook
            mouseProcDelegate = new MouseProc(MouseHookProc);
            hookHandle = SetWindowsHookEx(WH_MOUSE_LL, mouseProcDelegate, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

            lastClickTime = DateTime.Now; // Initialize lastClickTime

            // Add the event handler for button3 click (for Copy/Save Logs)
            button3.Click += Button3_Click;

            // Add the event handler for button6 click (redirect to GitHub)
            button6.Click += Button6_Click;

            // Add the event handler for button4 click (No Update Needed popup)
            button4.Click += Button4_Click;

            button5.Click += button5_Click;

            // Initialize NotifyIcon for system tray
            InitializeTrayIcon();
        }

        // Initialize NotifyIcon
        private void InitializeTrayIcon()
        {
            // Create context menu for the tray icon
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, Exit_Click);

            // Create the NotifyIcon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "FakeZeqaDcPrevent";  // Tooltip text when hovering over the icon
            trayIcon.Icon = new System.Drawing.Icon("appicon.ico");  // Your icon file for the tray
            trayIcon.Visible = false;  // Initially hidden
            trayIcon.DoubleClick += TrayIcon_DoubleClick;  // Restore window when tray icon is double-clicked
        }

        // Event handler for button5 (Hide to tray)
        private void button5_Click(object sender, EventArgs e)
        {
            // Hide the form and show the tray icon
            this.Hide();
            trayIcon.Visible = true;
            this.WindowState = FormWindowState.Minimized;  // Minimize the form to avoid showing in taskbar
        }

        // Event handler for tray icon double-click (Restore the window)
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;  // Restore the window to its normal state
            trayIcon.Visible = false;  // Hide the tray icon after restoring the window
        }

        // Event handler for "Exit" menu item click
        private void Exit_Click(object sender, EventArgs e)
        {
            trayIcon.Visible = false;  // Hide the tray icon before exiting
            Application.Exit();
        }

        // Event handler for Form Load
        private void Form1_Load(object sender, EventArgs e)
        {
            // Get the current date and time
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Define the log text
            string logText = $"[{currentDateTime}] First Opened{Environment.NewLine}" +
                             $"[{currentDateTime}] Current version: 1.1.8{Environment.NewLine}" +
                             $"[{currentDateTime}] Server version: 1.1.8{Environment.NewLine}";

            // Set the text in textBox3 (assuming textBox3 is for the mouse logs)
            textBox3.Text = logText;
        }

        // Event handler for button4 click (No Update Needed popup)
        private void Button4_Click(object sender, EventArgs e)
        {
            // Define the message content
            string message = "You are up to date\n" +
                             "Current version: 1.1.8\n" +
                             "Server version: 1.1.8";

            // Show a MessageBox with the title and message
            MessageBox.Show(message, "No Update Needed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Event handler for button6 click (redirect to GitHub)
        private void Button6_Click(object sender, EventArgs e)
        {
            // Open the URL in the default web browser
            System.Diagnostics.Process.Start("https://github.com/Zeqa-Network/DCPrevent");
        }

        // Event handler for button3 click (Context menu for Copy/Save Logs)
        private void Button3_Click(object sender, EventArgs e)
        {
            // Show the context menu with options for "Copy Logs" and "Save Logs"
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyLogsItem = new ToolStripMenuItem("Copy Logs");
            ToolStripMenuItem saveLogsItem = new ToolStripMenuItem("Save Logs");

            copyLogsItem.Click += CopyLogsItem_Click;
            saveLogsItem.Click += SaveLogsItem_Click;

            contextMenu.Items.Add(copyLogsItem);
            contextMenu.Items.Add(saveLogsItem);

            // Show context menu
            contextMenu.Show(button3, new System.Drawing.Point(0, button3.Height));
        }

        // Event handler for Copy Logs
        private void CopyLogsItem_Click(object sender, EventArgs e)
        {
            string logs = textBox3.Text; // Assuming textBox3 contains the logs

            if (!string.IsNullOrEmpty(logs))
            {
                // Copy logs to clipboard
                Clipboard.SetText(logs);

                // Get the current time when the log is copied
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Log the message in textBox3 with the timestamp
                textBox3.AppendText($"[{currentTime}] Logs copied to clipboard{Environment.NewLine}");

                // Display a MessageBox confirming the action
                MessageBox.Show("Logs copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No logs available to copy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for Save Logs
        private void SaveLogsItem_Click(object sender, EventArgs e)
        {
            string logs = textBox3.Text; // Assuming textBox3 contains the logs
            string userName = Environment.UserName; // Get the current user name
            string folderPath = Path.Combine(@"C:\Users\", userName, @"AppData\Roaming\BetterDCPrevent\Zeqa\logs");
            string fileName = "logs.txt"; // Set the file name for the logs

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath); // Ensure the directory exists
            }

            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, logs); // Save the logs to the file

            // Show the context menu with options to open the file, open the folder, or cancel
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem openFileItem = new ToolStripMenuItem("Open File");
            ToolStripMenuItem openFolderItem = new ToolStripMenuItem("Open Folder");
            ToolStripMenuItem cancelItem = new ToolStripMenuItem("Cancel");

            // Add click event handlers for each option
            openFileItem.Click += (s, ev) => System.Diagnostics.Process.Start(filePath); // Open the file
            openFolderItem.Click += (s, ev) => System.Diagnostics.Process.Start("explorer.exe", folderPath); // Open the folder
            cancelItem.Click += (s, ev) => { }; // Do nothing on cancel

            // Add the items to the context menu
            contextMenu.Items.Add(openFileItem);
            contextMenu.Items.Add(openFolderItem);
            contextMenu.Items.Add(cancelItem);

            // Show the context menu
            contextMenu.Show(button3, new System.Drawing.Point(0, button3.Height));
        }

        // Event handler for TrackBar1 scroll event
        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            // Update the TextBox with the current value of the first TrackBar
            textBox1.Text = trackBar1.Value.ToString();

            // If the checkbox is checked, sync trackBar2's value with trackBar1
            if (checkBox2.Checked)
            {
                trackBar2.Value = trackBar1.Value;
                textBox2.Text = trackBar2.Value.ToString();
            }
        }

        // Event handler for Button1 click event (Reset for trackBar1)
        private void Button1_Click(object sender, EventArgs e)
        {
            // Reset the first TrackBar to 50
            trackBar1.Value = 50;

            // Update the TextBox to reflect the new value of the first TrackBar
            textBox1.Text = trackBar1.Value.ToString();
        }

        // Event handler for TrackBar2 scroll event
        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            // Update the TextBox with the current value of the second TrackBar
            textBox2.Text = trackBar2.Value.ToString();
        }

        // Event handler for Button2 click event (Reset for trackBar2)
        private void Button2_Click(object sender, EventArgs e)
        {
            // Reset the second TrackBar to 50
            trackBar2.Value = 50;

            // Update the TextBox to reflect the new value of the second TrackBar
            textBox2.Text = trackBar2.Value.ToString();
        }

        // Event handler for checkBox2 CheckedChanged event
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                // Sync trackBar2's value with trackBar1 when checkbox is checked
                trackBar2.Value = trackBar1.Value;
                textBox2.Text = trackBar2.Value.ToString();
            }
        }

        // Event handler for checkBox3 CheckedChanged event (Lock functionality)
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                // Display a confirmation dialog
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to lock the controls? This action cannot be undone until you restart the program.",
                    "Lock Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // Lock the UI by disabling all interactive controls
                    LockControls(true);
                }
                else
                {
                    // If No is clicked, uncheck the checkbox and do nothing
                    checkBox3.Checked = false;
                }
            }
        }

        // Function to lock/unlock all controls in the form
        private void LockControls(bool lockControls)
        {
            // Disable/Enable the controls based on lockControls flag
            trackBar1.Enabled = !lockControls;
            trackBar2.Enabled = !lockControls;
            Button1.Enabled = !lockControls;
            button2.Enabled = !lockControls;
            checkBox1.Enabled = !lockControls;
            checkBox2.Enabled = !lockControls;
            checkBox3.Enabled = !lockControls;

            // Disable/Enable the text boxes based on lockControls flag
            textBox1.Enabled = !lockControls;
            textBox2.Enabled = !lockControls;
        }

        // Global mouse hook procedure
        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)  // Only process when nCode >= 0
            {
                // Check if the mouse event is a left-click
                if ((int)wParam == 0x201) // 0x201 = WM_LBUTTONDOWN
                {
                    // Get the current time when the click happened (formatted as HH:mm:ss)
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Get the time difference in milliseconds since the last click
                    TimeSpan timeDiff = DateTime.Now - lastClickTime;

                    // Get the value of trackBar1 (in ms)
                    int trackBarValue = trackBar1.Value;

                    // Format the message and display it in textBox3
                    textBox3.AppendText($"[{currentTime}] [{trackBarValue}ms] Detected left click (+{timeDiff.TotalMilliseconds:F0}ms){Environment.NewLine}");

                    // Update the last click time
                    lastClickTime = DateTime.Now;
                }
            }

            // Pass the event to the next hook in the chain
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        // When the form is closing, remove the hook
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
            base.OnFormClosing(e);
        }
    }
}
