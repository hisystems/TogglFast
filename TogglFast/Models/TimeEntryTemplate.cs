using System;
using System.Linq;
using System.Windows.Forms;

namespace ToggleFast.Models
{
    public class TimeEntryTemplate
    {
        public Toggl.Workspace Workspace { get; set; }
        public Toggl.Project Project { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool ExcludeWeekends { get; set; }

        public TimeEntryTemplate()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;

            StartTime = DateTime.Today.AddHours(9);
            EndTime = DateTime.Today.AddHours(17);
        }

        public void Validate()
        {
            if (StartDate > EndDate)
                throw new ArgumentException("Start date cannot be after the end date");
            else if (StartTime > EndTime)
                throw new ArgumentException("Start time cannot be after the end time");
            else if (Workspace == null)
                throw new ArgumentException("Workspace has not been selected");
            else if (Project == null)
                throw new ArgumentException("Project has not been selected");
        }

        public int DaysEntries
        {
            get
            {
                return ((int)(EndDate - StartDate).TotalDays) + 1;
            }
        }

        public Toggl.TimeEntry[] GenerateEntries()
        {
            return 
                Enumerable.Range(0, DaysEntries)
                .Select(index => StartDate.Date.AddDays(index))
                .Where(startDateTime => !ExcludeWeekends || (startDateTime.DayOfWeek != DayOfWeek.Saturday && startDateTime.DayOfWeek != DayOfWeek.Sunday))
                .Select(startDateTime =>
                    new Toggl.TimeEntry()
                    {
                        Start = (startDateTime + StartTime.TimeOfDay).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
                        Duration = (long)(EndTime - StartTime).TotalSeconds,
                        Description = this.Description,
                        IsBillable = true,
                        ProjectId = this.Project.Id,
                        WorkspaceId = this.Workspace.Id,
                        CreatedWith = Application.ProductName
                    }
                )
                .ToArray();
        }
    }
}
