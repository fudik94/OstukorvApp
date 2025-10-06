using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic; 

namespace OstukorvApp
{
    public partial class MainWindow : Window
    {
        // nashi dannie
        public struct Toode
        {
            public string Nimi { get; set; }
            public double Kogus { get; set; }
            public double Hind { get; set; }
            public bool Ostetud { get; set; }
        }

        private List<Toode> tooted = new List<Toode>();

        // eto nashi preduprejdeniya
        private HashSet<string> shownWarnings = new HashSet<string>();

        public MainWindow()
        {
            InitializeComponent();
            ToodedGrid.ItemsSource = tooted;
        }

        // dobavim nov tovar
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string nimi = Interaction.InputBox("Sisesta toote nimi:", "Lisa toode");
            if (string.IsNullOrWhiteSpace(nimi)) return;

            string kogusStr = Interaction.InputBox("Sisesta kogus:", "Lisa toode", "1");
            if (!double.TryParse(kogusStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double kogus))
                kogus = 1;

            string hindStr = Interaction.InputBox("Sisesta hind (€):", "Lisa toode", "0");
            if (!double.TryParse(hindStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double hind))
                hind = 0;

            Toode uusToode = new Toode { Nimi = nimi, Kogus = kogus, Hind = hind, Ostetud = false };
            tooted.Add(uusToode);

            // proverka predub
            CheckWarnings(uusToode);

            RefreshGrid();
        }

        // proverka predub
        private void CheckWarnings(Toode t)
        {
            string nimiLower = t.Nimi.ToLower();

            var messages = new Dictionary<string, string>()
            {
                {"liha", "Näen, sa ei ole taimetoitlaste sõber 😏"},
                {"kana", "Näen, sa ei ole taimetoitlaste sõber 😏"},
                {"kala", "Näen, sa ei ole taimetoitlaste sõber 😏"},
                {"cola", "Parem võta Zero, seal pole suhkrut 🥤"},
                {"malbro", "Kas tead, mitu inimest suitsetamise tõttu iga aasta sureb? 🚬"},
                {"marlboro", "Kas tead, mitu inimest suitsetamise tõttu iga aasta sureb? 🚬"},
                {"alcohol", "Sul on ikka vähemalt 18 aastat? 🍺"},
                {"šokolaad", "Ära liialda suhkruga, hambad tänavad sind 🍫"},
                {"chocolate", "Ära liialda suhkruga, hambad tänavad sind 🍫"},
                {"chips", "Võiks ka köögivilju lisada 🥦"},
                {"pizza", "Kiirtoit — maitsev, aga mõtle tervisele 🍕"},
                {"burger", "Kiirtoit — maitsev, aga mõtle tervisele 🍔"},
                {"fanta", "Suhkur voolab su vereringesse ⚡"}
            };

            foreach (var kvp in messages)
            {
                if (nimiLower.Contains(kvp.Key) && !shownWarnings.Contains(kvp.Key))
                {
                    MessageBox.Show(kvp.Value, "Hoiatus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    shownWarnings.Add(kvp.Key);
                    break;
                }
            }

            // proverka summi
            double kokku = tooted.Sum(x => x.Hind * x.Kogus);
            if (kokku >= 500 && !shownWarnings.Contains("summa500"))
            {
                MessageBox.Show("Oi, võiks teise rahakoti hankida 😅", "Hoiatus", MessageBoxButton.OK, MessageBoxImage.Warning);
                shownWarnings.Add("summa500");
            }
            else if (kokku >= 345 && !shownWarnings.Contains("summa345"))
            {
                MessageBox.Show("Oled juba kulutanud rohkem kui Eesti elatusmiinimum 💸", "Hoiatus", MessageBoxButton.OK, MessageBoxImage.Warning);
                shownWarnings.Add("summa345");
            }

            // proverka kol bolshe 10 kq
            if (t.Kogus > 10 && !shownWarnings.Contains("kogus10"))
            {
                MessageBox.Show("Tõesti kavatsed nii palju süüa? 😳", "Hoiatus", MessageBoxButton.OK, MessageBoxImage.Warning);
                shownWarnings.Add("kogus10");
            }
        }

        // delete tovar
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToodedGrid.SelectedItem is Toode valitud)
            {
                var tulemus = MessageBox.Show(
                    "Kas olete kindel, et soovite eemaldada selle toote?",
                    "Kinnitus",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (tulemus == MessageBoxResult.Yes)
                {
                    tooted.Remove(valitud);
                    RefreshGrid();
                }
            }
            else
            {
                MessageBox.Show("Palun vali toode, mida eemaldada.");
            }
        }

        // edit tovar
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToodedGrid.SelectedItem is Toode valitud)
            {
                string uusNimi = Interaction.InputBox("Uus nimi:", "Muuda toode", valitud.Nimi);

                string uusKogusStr = Interaction.InputBox("Uus kogus:", "Muuda toode", valitud.Kogus.ToString(CultureInfo.InvariantCulture));
                if (!double.TryParse(uusKogusStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double uusKogus))
                    uusKogus = valitud.Kogus;

                string uusHindStr = Interaction.InputBox("Uus hind (€):", "Muuda toode", valitud.Hind.ToString(CultureInfo.InvariantCulture));
                if (!double.TryParse(uusHindStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double uusHind))
                    uusHind = valitud.Hind;

                int index = tooted.IndexOf(valitud);
                tooted[index] = new Toode
                {
                    Nimi = uusNimi,
                    Kogus = uusKogus,
                    Hind = uusHind,
                    Ostetud = valitud.Ostetud
                };

                CheckWarnings(tooted[index]);

                RefreshGrid();
            }
            else
            {
                MessageBox.Show("Palun vali toode, mida muuta.");
            }
        }

        // sortirovka alfavit
        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            tooted = tooted.OrderBy(t => t.Nimi).ToList();
            RefreshGrid();
        }

        // obnov i summa
        private void RefreshGrid()
        {
            ToodedGrid.ItemsSource = null;
            ToodedGrid.ItemsSource = tooted;

            double kokku = tooted.Sum(t => t.Hind * t.Kogus);
            TotalLabel.Text = $"{kokku:F2} €";
        }

        // poisk po imeni
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string otsing = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(otsing))
            {
                ToodedGrid.ItemsSource = tooted;
                return;
            }

            var filtritud = tooted.Where(t => t.Nimi.ToLower().Contains(otsing)).ToList();
            ToodedGrid.ItemsSource = filtritud;
        }

        // qalochka kak mark
        private void ToodedGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => RefreshGrid()));
        }
    }
}
