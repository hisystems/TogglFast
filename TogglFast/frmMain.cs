using System;
using System.Windows.Forms;
using ToggleFast.Models;
using ToggleFast.Properties;

namespace ToggleFast
{
    public partial class frmMain : Form
    {
        private TimeEntryTemplate template;

        public frmMain()
        {
            InitializeComponent();

            this.template = new TimeEntryTemplate();

            this.dtpStartTime.Value = this.template.StartTime;
            this.dtpEndTime.Value = this.template.EndTime;

            this.template.ExcludeWeekends = true;

            this.txtApiToken.Text = Settings.Default.ApiToken;
        }

        private void btnGenerateEntries_Click(object sender, EventArgs e)
        {
            if (this.IsValid())
            {
                var entries = template.GenerateEntries();

                if (MessageBox.Show($"Are you sure you want to generate {entries.Length} timesheet entries?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        var api = new Toggl.Toggl(this.txtApiToken.Text);

                        foreach (var item in entries)
                            api.TimeEntry.Add(item);

                        MessageBox.Show($"Entries successfully added.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occured adding the time entry. {ex.Message}.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private bool IsValid()
        {
            if (String.IsNullOrEmpty(this.txtApiToken.Text))
            {
                MessageBox.Show("API token is not set. Go to 'My Profile' and copy the 'API Token' value.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                this.template.Validate();
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void txtApiToken_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtApiToken.Text))
            {
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    var api = new Toggl.Toggl(txtApiToken.Text);

                    cmbProject.Items.AddRange(api.Project.List().ToArray());
                    if (cmbProject.Items.Count > 0)
                        cmbProject.SelectedIndex = 0;

                    cmbWorkspace.Items.AddRange(api.Workspace.List().ToArray());
                    if (cmbWorkspace.Items.Count > 0)
                        cmbWorkspace.SelectedIndex = 0;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void cmbWorkspace_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.template.Workspace = (Toggl.Workspace)cmbWorkspace.SelectedItem;
        }

        private void cmbProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.template.Project = (Toggl.Project)cmbProject.SelectedItem;
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            this.template.Description = txtDescription.Text;
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            this.template.StartDate = dtpStartDate.Value;
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            this.template.EndDate = dtpEndDate.Value;
        }

        private void dtpStartTime_ValueChanged(object sender, EventArgs e)
        {
            this.template.StartTime = dtpStartTime.Value;
        }

        private void dtpEndTime_ValueChanged(object sender, EventArgs e)
        {
            this.template.EndTime = dtpEndTime.Value;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }

        private void txtApiToken_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtApiToken.Text))
                Settings.Default.ApiToken = txtApiToken.Text;
        }

        private void chkExcludeWeekends_CheckedChanged(object sender, EventArgs e)
        {
            this.template.ExcludeWeekends = chkExcludeWeekends.Checked;
        }
    }
}
