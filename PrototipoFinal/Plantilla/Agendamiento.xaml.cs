using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PrototipoFinal.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.Plantilla
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class Agendamiento : Page
    {
        private DateTime currentMonth = DateTime.Now;
        private List<Appointment> appointments = new List<Appointment>();
        private DateTime selectedDate;

        public Agendamiento()
        {
            this.InitializeComponent();
            UpdateCalendar();
            AreaComboBox.SelectionChanged += AreaComboBox_SelectionChanged;
        }

        private void UpdateCalendar()
        {
            CurrentMonthText.Text = currentMonth.ToString("MMMM yyyy");

            // Clear existing grid
            DaysGrid.Children.Clear();
            DaysGrid.RowDefinitions.Clear();
            DaysGrid.ColumnDefinitions.Clear();

            // Setup grid
            for (int i = 0; i < 6; i++)
                DaysGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 7; i++)
                DaysGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Get first day of month
            DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            int offset = ((int)firstDay.DayOfWeek + 6) % 7; // Adjust for Monday start

            // Create calendar buttons
            int currentDay = 1;
            int totalDays = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if (row == 0 && col < offset || currentDay > totalDays)
                        continue;

                    Button dayButton = new Button
                    {
                        Style = (Style)Resources["CalendarDayStyle"],
                        Content = new StackPanel
                        {
                            Children =
                        {
                            new TextBlock
                            {
                                Text = currentDay.ToString(),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }
                        }
                    };

                    DateTime buttonDate = new DateTime(currentMonth.Year, currentMonth.Month, currentDay);
                    dayButton.Tag = buttonDate;

                    // Disable past dates and Sundays
                    if (buttonDate < DateTime.Today || buttonDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dayButton.IsEnabled = false;
                        dayButton.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        dayButton.Click += DayButton_Click;
                    }

                    Grid.SetRow(dayButton, row);
                    Grid.SetColumn(dayButton, col);
                    DaysGrid.Children.Add(dayButton);
                    currentDay++;
                }
            }

            UpdateAppointmentIndicators();
        }

        private void UpdateAppointmentIndicators()
        {
            foreach (Button button in DaysGrid.Children.OfType<Button>())
            {
                if (button.Tag is DateTime buttonDate)
                {
                    var dayAppointments = appointments.Where(a =>
                        a.Date.Date == buttonDate.Date &&
                        a.Status != "Cancelled" &&
                        a.Area == ((ComboBoxItem)AreaComboBox.SelectedItem)?.Content.ToString()
                    ).ToList();

                    if (dayAppointments.Any())
                    {
                        ((StackPanel)button.Content).Children.Add(new TextBlock
                        {
                            Text = $"{dayAppointments.Count} citas",
                            Foreground = new SolidColorBrush(Colors.Green),
                            HorizontalAlignment = HorizontalAlignment.Center
                        });
                    }
                }
            }
        }

        private async void DayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            selectedDate = (DateTime)button.Tag;

            // Generate available time slots
            TimeComboBox.Items.Clear();
            var availableSlots = GenerateAvailableTimeSlots(selectedDate);
            foreach (var slot in availableSlots)
            {
                TimeComboBox.Items.Add(slot.Time.ToString(@"hh\:mm"));
            }

            if (TimeComboBox.Items.Count == 0)
            {
                await new ContentDialog
                {
                    Title = "No hay horarios disponibles",
                    Content = "No hay horarios disponibles para este día.",
                    CloseButtonText = "OK"
                }.ShowAsync();
                return;
            }

            AppointmentDialog.Title = $"Agendar cita para {selectedDate:dd/MM/yyyy}";
            var result = await AppointmentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                CreateAppointment();
                UpdateCalendar();
            }
        }

        private List<TimeSlot> GenerateAvailableTimeSlots(DateTime date)
        {
            var slots = new List<TimeSlot>();
            var startTime = new TimeSpan(14, 0, 0); // 2 PM
            var endTime = new TimeSpan(18, 0, 0); // 6 PM
            var interval = TimeSpan.FromMinutes(30);

            var currentArea = ((ComboBoxItem)AreaComboBox.SelectedItem)?.Content.ToString();

            while (startTime < endTime)
            {
                // Check if slot is already taken
                var isSlotTaken = appointments.Any(a =>
                    a.Date.Date == date.Date &&
                    a.Time == startTime &&
                    a.Status != "Cancelled");

                if (!isSlotTaken)
                {
                    slots.Add(new TimeSlot
                    {
                        Date = date,
                        Time = startTime,
                        IsAvailable = true,
                        Area = currentArea
                    });
                }

                startTime = startTime.Add(interval);
            }

            return slots;
        }

        private void CreateAppointment()
        {
            var selectedTime = TimeSpan.Parse(TimeComboBox.SelectedItem.ToString());
            var appointment = new Appointment
            {
                Id = appointments.Count + 1,
                Date = selectedDate,
                Time = selectedTime,
                Area = ((ComboBoxItem)AreaComboBox.SelectedItem)?.Content.ToString(),
                PatientName = PatientNameBox.Text,
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                Reason = ReasonBox.Text
            };

            appointments.Add(appointment);

            // Clear form
            PatientNameBox.Text = "";
            EmailBox.Text = "";
            PhoneBox.Text = "";
            ReasonBox.Text = "";
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMonth > DateTime.Today)
            {
                currentMonth = currentMonth.AddMonths(-1);
                UpdateCalendar();
            }
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            UpdateCalendar();
        }

        private void AreaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalendar();
        }
        private async void ViewAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (appointments.Count == 0)
            {
                await new ContentDialog
                {
                    Title = "Sin Citas",
                    Content = "No hay citas agendadas.",
                    CloseButtonText = "OK"
                }.ShowAsync();
                return;
            }

            var content = new StackPanel();

            foreach (var appointment in appointments)
            {
                content.Children.Add(new TextBlock
                {
                    Text = $"{appointment.Date:dd/MM/yyyy} - {appointment.Time} - {appointment.PatientName} - {appointment.Area}",
                    Margin = new Thickness(0, 5, 0, 5)
                });
            }

            var dialog = new ContentDialog
            {
                Title = "Citas Agendadas",
                Content = content,
                CloseButtonText = "Cerrar"
            };

            await dialog.ShowAsync();
        }
    }
}