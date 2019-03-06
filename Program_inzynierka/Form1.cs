using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Program_inzynierka
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.SmallImageList = imageList1;
        }
        //zmienna do zapisywania nazwy wybranego pliku
        public string name_of_csv_file = "";
        //zmienna do zapisania wybranego rodzaju udaru
        string operation_rodzaj_udaru = "Nap";
        //zmienna do zapisania wybranej metody
        string operation_rodzaj_metody = "Norma";
        //zmienna do zapisania wybranego oscyloskopu
        string operation_rodzaj_oscyl = "Model1";

        //inicjalizacja wektora napięcia (lub prądu)
        List<double> U = new List<double>();
        //inicjalizacja wektora czasu
        List<double> t = new List<double>();

        /// Wybor rodzaju udaru///
        private void Operator_click(object sender, EventArgs e)
        {
            if (U_nap.CheckState == CheckState.Checked)
            {
                operation_rodzaj_udaru = "Nap";
                U_prad.Enabled = false;
            }
            else if (U_prad.CheckState == CheckState.Checked)
            {
                operation_rodzaj_udaru = "Prad";
                U_nap.Enabled = false;
            }
            else if (U_nap.CheckState == CheckState.Unchecked && U_prad.CheckState == CheckState.Unchecked)
            {
                U_nap.Enabled = true;
                U_prad.Enabled = true;
            }
        }
        ///Wybor rodzaju normy///
        private void Operator_clock(object sender, EventArgs e)
        {
            if (Met_norm.CheckState == CheckState.Checked)
            {
                operation_rodzaj_metody = "Norma";
                Met_alter.Enabled = false;
            }
            else if (Met_alter.CheckState == CheckState.Checked)
            {
                operation_rodzaj_metody = "Alter";
                Met_norm.Enabled = false;
            }
            else if (Met_norm.CheckState == CheckState.Unchecked && Met_alter.CheckState == CheckState.Unchecked)
            {
                Met_norm.Enabled = true;
                Met_alter.Enabled = true;
            }
        }
        ///Wybor rodzaju oscyloskopu///
        private void Operator_cluck(object sender, EventArgs e)
        {
            if (Mod_1.CheckState == CheckState.Checked)
            {
                operation_rodzaj_oscyl = "Model1";
                Mod_mso4054.Enabled = false;
            }
            else if (Mod_mso4054.CheckState == CheckState.Checked)
            {
                operation_rodzaj_oscyl = "Modelmso4054";
                Mod_1.Enabled = false;
            }
            else if (Mod_1.CheckState == CheckState.Unchecked && Mod_mso4054.CheckState == CheckState.Unchecked)
            {
                Mod_1.Enabled = true;
                Mod_mso4054.Enabled = true;
            }
        }
        ///
        ///Wybranie pliku///
        public void Wybor_pliku()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    string filter = "csv files (*.csv*)|*.csv*";

                    openFileDialog.Filter = filter;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        name_of_csv_file = openFileDialog.FileName;

                        ListViewItem lvi = new ListViewItem();

                        lvi.Text = name_of_csv_file.Remove(0, name_of_csv_file.LastIndexOf('\\') + 1);
                        lvi.ImageIndex = 0; // установка картинки для файла
                                            // добавляем элемент в ListView
                        listView1.Items.Add(lvi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //inicjalizacja zmiennej dla waartości przekładni
        double przekladnia = 1;


        ///Dwa algorytmy do odczytania pliku///
        [STAThread]
        public void Odczyt_pliku_i_obliczanie()
        {
            try
            {
                przekladnia = Convert.ToDouble(textBox4.Text);
                StreamReader objReader = new StreamReader(name_of_csv_file);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                int i = 0;
                string[] l = new string[] { };
                string s = "";
                string s1 = "";
                string v = "";
                string sLine = "";
                while (sLine != null)
                {
                    //Algorytm do odczytania pliku z oscyloskopu Tektronics MSO4054
                    if (operation_rodzaj_oscyl == "Modelmso4054")
                    {
                        sLine = objReader.ReadLine();
                        i++;
                        //zaczyna się odczytanie wartości liczbowych od 16 wierszu
                        if (sLine != null && i > 15)
                        {
                            //Wyodrębnienie wektorów napięcia (lub prądu) i czasu 
                            s1 = sLine.Replace(',', ' ');
                            l = s1.Split(new char[] { ' ' }).ToArray();
                            if (l[0] != "" && l[1] != "")
                            {
                                t.Add(double.Parse(l[0]) * przekladnia);
                                U.Add(double.Parse(l[1]) * przekladnia);
                            }
                        }
                    }
                    //Algorytm do odczytania pliku z oscyloskopu Tektronics TDS2012B
                    else if (operation_rodzaj_oscyl == "Model1")
                    {
                        sLine = objReader.ReadLine();
                        i++;
                        //zaczyna się odczytanie wartości liczbowych od 16 wierszu
                        if (sLine != null && i > 18)
                        {
                            //Wyodrębnienie wektorów napięcia (lub prądu) i czasu 
                            s = sLine.Replace(';', ' ');
                            v = s.Replace(",", "");
                            l = v.Split(new char[] { ' ' }).ToArray();
                            if (l[0] != "" && l[2] != "")
                            {
                                t.Add(double.Parse(l[0]) * przekladnia);
                                U.Add(double.Parse(l[2]) * przekladnia);
                            }
                            else if (l[0] != "" && l[3] != "")
                            {
                                t.Add(double.Parse(l[0]) * przekladnia);
                                U.Add(double.Parse(l[3]) * przekladnia);
                            }
                        }
                    }
                }
                objReader.Close();


                ////////////obliczanie//////////////
                try
                {
                    //inicjalizacja zmienne do czasu trwania czoła
                    double T1 = 0;
                    //inicjalizacja początku czasu trwania czołą 
                    double poczatek_trwania_czola = 0;
                    //inicjalizacja wspołczynniku kierunkowego prostej
                    double wsp_kierunk_prost = 0;
                    //inicjalizacja przesunięcia prostej
                    double Przesuniecie = 0;
                    //inicjalizacja końca trwania czołą
                    double koniec_trwania_czola = 0;
                    //inicjalizacja czasu odpowiadającego 10% wartości szczytowej
                    double t01 = 0;
                    //inicjalizacja czasu odpowiadającego 90% wartości szczytowej
                    double t09 = 0;
                    //inicjalizacja czasu odpowiadającego 30% wartości szczytowej
                    double t03 = 0;
                    //inicjalizacja pomocniczej zmiennej dla metody alternatywnej
                    double roznica = 0;

                    List<double> U_list1 = new List<double>();
                    List<double> Ua_list = new List<double>();
                    List<double> Ua_list1 = new List<double>();
                    //inicjalizacja kolekcji wektora napięcia (lub prądu) od początku do wartości szczytowej
                    List<double> U_list_first_side = new List<double>();

                    for (int ii = 0; ii < t.Count; ii++)
                    {
                        U_list1.Add(U[ii]);
                        Ua_list.Add(U[ii]);
                        Ua_list1.Add(U[ii]);
                        U_list_first_side.Add(U[ii]);
                    }
                    //inicjalizacja wartości szczytowej
                    double Wart_szczyt = U.Max();
                    double indeks = U.Max();
                    //inicjalizacja indeksu wartości szczytowej
                    int ind_szczyt = U.IndexOf(indeks);


                    U_list_first_side.RemoveRange(++ind_szczyt, U_list_first_side.Count - ind_szczyt);


                    //inicjalizacja zmiennej 90% wartości szczytowej
                    double Wart_szczyt_90proc = 0.9 * Wart_szczyt;

                    //inicjalizacja kolekcji indeksów wektora napięcia lub prądu mniejszych niz 90% wartości szczytowej
                    List<int> Indeksy_Wart_szczyt_90proc_mn = new List<int>();
                    //inicjalizacja kolekcji indeksów wektora napięcia lub prądu większych niz 90% wartości szczytowej
                    List<int> Indeksy_Wart_szczyt_90proc_wieksz = new List<int>();

                    //Dodania wartości do kolekcji
                    foreach (double n in U_list_first_side)
                    {
                        if (n >= Wart_szczyt_90proc)
                        {
                            Indeksy_Wart_szczyt_90proc_wieksz.Add(U_list_first_side.IndexOf(n));
                        }
                        else if (n <= Wart_szczyt_90proc)
                        {
                            Indeksy_Wart_szczyt_90proc_mn.Add(U_list_first_side.IndexOf(n));
                        }
                    }
                    //zmienna, która ma maksymalną wartość z kolekcji Indeksy_Wart_szczyt_90proc_mn
                    double Max_ind_90proc_mn = U_list_first_side[Indeksy_Wart_szczyt_90proc_mn.Max()];
                    //zmienna, która ma minimalną wartość z kolekcji Indeksy_Wart_szczyt_90proc_wieksz
                    double Min_ind_90proc_wieksz = U_list_first_side[Indeksy_Wart_szczyt_90proc_wieksz.Min()];

                    //sprawdzenie równości określonych zmiennych powyżej z 90% wartości szczytowej
                    if (Max_ind_90proc_mn == Wart_szczyt_90proc)
                    {
                        t09 = t[Indeksy_Wart_szczyt_90proc_mn.Max()];
                    }
                    else if (Min_ind_90proc_wieksz == Wart_szczyt_90proc)
                    {
                        t09 = t[Indeksy_Wart_szczyt_90proc_wieksz.Min()];
                    }
                    //przy nierówności - obliczenie graficzne czasu odpowiadającego 90% wartości szczytowej
                    else
                    {
                        t09 = ((Wart_szczyt_90proc - Max_ind_90proc_mn) * (t[Indeksy_Wart_szczyt_90proc_wieksz.Min()] - t[Indeksy_Wart_szczyt_90proc_mn.Max()]) / (Min_ind_90proc_wieksz - Max_ind_90proc_mn)) + (t[Indeksy_Wart_szczyt_90proc_mn.Max()]);
                    }

                    //algorytm obliczeniowy dla udaru napięciowego
                    if (operation_rodzaj_udaru == "Nap")
                    {
                        //inicjalizacja zmiennej 30% wartości szczytowej
                        double Wart_szczyt_30proc = 0.3 * Wart_szczyt;

                        //inicjalizacja kolekcji indeksów wektora napięcia lub prądu mniejszych niz 30% wartości szczytowej
                        List<int> Indeksy_Wart_szczyt_30proc_mn = new List<int>();
                        //inicjalizacja kolekcji indeksów wektora napięcia lub prądu większych niz 30% wartości szczytowej
                        List<int> Indeksy_Wart_szczyt_30proc_wieksz = new List<int>();

                        //Dodania wartości do kolekcji
                        foreach (double n1 in U_list_first_side)
                        {
                            if (n1 >= Wart_szczyt_30proc)
                            {
                                Indeksy_Wart_szczyt_30proc_wieksz.Add(U_list_first_side.IndexOf(n1));
                            }
                            else if (n1 <= Wart_szczyt_30proc)
                            {
                                Indeksy_Wart_szczyt_30proc_mn.Add(U_list_first_side.IndexOf(n1));
                            }
                        }
                        //zmienna, która ma maksymalną wartość z kolekcji Indeksy_Wart_szczyt_30proc_mn
                        double Max_ind_30proc_mn = U_list_first_side[Indeksy_Wart_szczyt_30proc_mn.Max()];
                        //zmienna, która ma minimalną wartość z kolekcji Indeksy_Wart_szczyt_30proc_wieksz
                        double Min_ind_30proc_wieksz = U_list_first_side[Indeksy_Wart_szczyt_30proc_wieksz.Min()];


                        //sprawdzenie równości określonych zmiennych powyżej z 30% wartości szczytowej
                        if (Max_ind_30proc_mn == Wart_szczyt_30proc)
                        {
                            t03 = t[Indeksy_Wart_szczyt_30proc_mn.Max()];
                        }
                        else if (Min_ind_30proc_wieksz == Wart_szczyt_30proc)
                        {
                            t03 = t[Indeksy_Wart_szczyt_30proc_wieksz.Min()];
                        }
                        //przy nierówności - obliczenie graficzne czasu odpowiadającego 30% wartości szczytowej
                        else
                        {
                            t03 = ((Wart_szczyt_30proc - Max_ind_30proc_mn) * (t[Indeksy_Wart_szczyt_30proc_wieksz.Min()] - t[Indeksy_Wart_szczyt_30proc_mn.Max()]) / (Min_ind_30proc_wieksz - Max_ind_30proc_mn)) + (t[Indeksy_Wart_szczyt_30proc_mn.Max()]);
                        }
                        //zmienna dla czasu narastania czoła w metodzie alternatywnej dla udaru napięciowego
                        roznica = (t09 - t03) * 1.67;
                        //wyznaczenie współczynniku kierunkowego prostej 
                        wsp_kierunk_prost = (Wart_szczyt_90proc - Wart_szczyt_30proc) / (t09 - t03);
                        //wyznaczenie przesunięcia prostej
                        Przesuniecie = Wart_szczyt_30proc - (wsp_kierunk_prost * t03);
                        //Wyznaczenie początku czasu trwania czoła
                        poczatek_trwania_czola = -Przesuniecie / wsp_kierunk_prost;
                        //Wyznaczenie końca trwania czoła
                        koniec_trwania_czola = (Wart_szczyt - Przesuniecie) / wsp_kierunk_prost;
                    }
                    //algorytm do wyznaczenie parametrów udaru prądowego
                    else if (operation_rodzaj_udaru == "Prad")
                    {
                        //inicjalizacja zmiennej 10% wartości szczytowej
                        double Wart_szczyt_10proc = 0.1 * Wart_szczyt;

                        //inicjalizacja kolekcji indeksów wektora napięcia lub prądu mniejszych niz 10% wartości szczytowej
                        List<int> Indeksy_Wart_szczyt_10proc_mn = new List<int>();
                        //inicjalizacja kolekcji indeksów wektora napięcia lub prądu większych niz 10% wartości szczytowej
                        List<int> Indeksy_Wart_szczyt_10proc_wieksz = new List<int>();

                        //Dodania wartości do kolekcji
                        foreach (double n11 in U_list_first_side)
                        {
                            if (n11 >= Wart_szczyt_10proc)
                            {
                                Indeksy_Wart_szczyt_10proc_wieksz.Add(U_list_first_side.IndexOf(n11));
                            }
                            else if (n11 <= Wart_szczyt_10proc)
                            {
                                Indeksy_Wart_szczyt_10proc_mn.Add(U_list_first_side.IndexOf(n11));
                            }
                        }
                        //zmienna, która ma maksymalną wartość z kolekcji Indeksy_Wart_szczyt_10proc_mn
                        double Max_ind_10proc_mn = U_list_first_side[Indeksy_Wart_szczyt_10proc_mn.Max()];
                        //zmienna, która ma minimalną wartość z kolekcji Indeksy_Wart_szczyt_10proc_wieksz
                        double Min_ind_10proc_wieksz = U_list_first_side[Indeksy_Wart_szczyt_10proc_wieksz.Min()];


                        //sprawdzenie równości określonych zmiennych powyżej z 10% wartości szczytowej
                        if (Max_ind_10proc_mn == Wart_szczyt_10proc)
                        {
                            t01 = t[Indeksy_Wart_szczyt_10proc_mn.Max()];
                        }
                        else if (Min_ind_10proc_wieksz == Wart_szczyt_10proc)
                        {
                            t01 = t[Indeksy_Wart_szczyt_10proc_wieksz.Min()];
                        }
                        //przy nierówności - obliczenie graficzne czasu odpowiadającego 10% wartości szczytowej
                        else
                        {
                            t01 = ((Wart_szczyt_10proc - Max_ind_10proc_mn) * (t[Indeksy_Wart_szczyt_10proc_wieksz.Min()] - t[Indeksy_Wart_szczyt_10proc_mn.Max()]) / (Min_ind_10proc_wieksz - Max_ind_10proc_mn)) + (t[Indeksy_Wart_szczyt_10proc_mn.Max()]);
                        }
                        //zmienna dla czasu narastania czoła w metodzie alternatywnej dla udaru prądowego
                        roznica = (t09 - t01) * 1.25;
                        wsp_kierunk_prost = (Wart_szczyt_90proc - Wart_szczyt_10proc) / (t09 - t01);
                        Przesuniecie = Wart_szczyt_10proc - (wsp_kierunk_prost * t01);
                        poczatek_trwania_czola = -Przesuniecie / wsp_kierunk_prost;
                        koniec_trwania_czola = (Wart_szczyt - Przesuniecie) / wsp_kierunk_prost;

                    }
                    ///wybor rodzaju metody do obliczania parametrów charakterystycznych udaru///
                    //Wybór metody graficznej
                    if (operation_rodzaj_metody == "Norma")
                    {
                        T1 = koniec_trwania_czola - poczatek_trwania_czola;
                    }
                    //Wybór metody alternatywnej
                    else if (operation_rodzaj_metody == "Alter")
                    {
                        T1 = roznica;
                    }
                    ///
                    //inicjalizacja zmiennej 50% wartości szczytowej
                    double Wart_szczyt_50proc = 0.5 * Wart_szczyt;
                    //inicjalizacja kolekcji od wartości szczytowej do końca wektora
                    List<double> U_list_second_side = U_list1;
                    U_list_second_side.RemoveRange(0, --ind_szczyt);

                    //inicjalizacja kolekcji indeksów wektora napięcia lub prądu mniejszych niz 50% wartości szczytowej
                    List<int> Indeksy_Wart_szczyt_50proc_mn = new List<int>();
                    //inicjalizacja kolekcji indeksów wektora napięcia lub prądu większych niz 50% wartości szczytowej
                    List<int> Indeksy_Wart_szczyt_50proc_wieksz = new List<int>();

                    //Dodania wartości do kolekcji
                    foreach (double n in U_list_second_side)
                    {
                        if (n >= Wart_szczyt_50proc)
                        {
                            Indeksy_Wart_szczyt_50proc_mn.Add(U_list_second_side.IndexOf(n));
                        }
                        else if (n <= Wart_szczyt_50proc)
                        {
                            Indeksy_Wart_szczyt_50proc_wieksz.Add(U_list_second_side.IndexOf(n));
                        }

                    }
                    //zmienna, która ma maksymalną wartość z kolekcji Indeksy_Wart_szczyt_50proc_mn
                    double Max_ind_50proc_mn = U_list_second_side[Indeksy_Wart_szczyt_50proc_mn.Max()];
                    //zmienna, która ma minimalną wartość z kolekcji Indeksy_Wart_szczyt_50proc_wieksz
                    double Min_ind_50proc_wieksz = U_list_second_side[Indeksy_Wart_szczyt_50proc_wieksz.Min()];

                    //inicjalizacja czasu odpowiadającego 50% wartości szczytowej
                    double t05 = 0;
                    //sprawdzenie równości określonych zmiennych powyżej z 50% wartości szczytowej
                    if (Max_ind_50proc_mn == Wart_szczyt_50proc)
                    {
                        t05 = t[Indeksy_Wart_szczyt_50proc_mn.Max() + ind_szczyt];
                    }
                    else if (Min_ind_50proc_wieksz == Wart_szczyt_50proc)
                    {
                        t05 = t[Indeksy_Wart_szczyt_50proc_wieksz.Min() + ind_szczyt];
                    }
                    //przy nierówności - obliczenie graficzne czasu odpowiadającego 50% wartości szczytowej
                    else
                    {
                        t05 = ((Wart_szczyt_50proc - Max_ind_50proc_mn) * (t[Indeksy_Wart_szczyt_50proc_wieksz.Min() + ind_szczyt] - t[Indeksy_Wart_szczyt_50proc_mn.Max() + ind_szczyt]) / (Min_ind_50proc_wieksz - Max_ind_50proc_mn)) + (t[Indeksy_Wart_szczyt_50proc_mn.Max() + ind_szczyt]);
                    }

                    //wyznaczenie czasu do półszczytu
                    double T2 = t05 - poczatek_trwania_czola;

                    //wyświetlenie wartości czasów w interfejscie graficznym
                    textBox1.Text = T1.ToString("0.###E0");
                    textBox2.Text = T2.ToString("0.###E0");
                    textBox3.Text = Wart_szczyt.ToString("#.000");

                    if (textBox4.Text == "")
                    {
                        textBox4.BackColor = Color.Red;
                        MessageBox.Show("Wpisz wartość przekładni!");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Podany plik jest zmieniony! \nPoszę sprawdzić parzystość liczby próbek lub zawartość pliku na puste pola!");
                    Environment.Exit(1);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Wybrany oscyloskop nie pasuje!");

            }
        }

        public void Wykres(List<double> a, List<double> b)
        {

            chart1.Series[0].Points.Clear();
            //// chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, t_max);
            //chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            //chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            //chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;


            //////chart1.ChartAreas[0].AxisY.ScaleView.Zoom(0, U_max);
            //chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            //chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            //chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            //chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;

            if (b.Count > 11000 && b.Count < 55000)
            {
                for (int i = 0; i < b.Count; i = i + 5)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }
            else if (b.Count >= 55000 && b.Count < 110000)
            {
                for (int i = 0; i < b.Count; i = i + 10)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }
            else if (b.Count >= 110000 && b.Count < 510000)
            {
                for (int i = 0; i < b.Count; i = i + 50)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }
            else if (b.Count >= 510000 && b.Count < 1100000)
            {
                for (int i = 0; i < b.Count; i = i + 100)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }
            else if (b.Count >= 9100000 && b.Count < 11100000)
            {
                for (int i = 0; i < b.Count; i = i + 1000)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }
            else
            {
                for (int i = 0; i < b.Count; i++)
                {
                    chart1.Series[0].Points.AddXY(a[i], b[i]);
                }
            }

            Axis ax = new Axis();
            ax.Title = "Czas t [s]";
            chart1.ChartAreas[0].AxisX = ax;
            Axis ay = new Axis();
            if (operation_rodzaj_udaru == "Nap")
            {
                ay.Title = "Napięcie U [kV]";
                label11.Text = "[kV]";
                label7.Text = "Maksymalna wartość napięcie  Um: ";
            }
            else
            {
                ay.Title = "Prąd I [kA]";
                label11.Text = "[kA]";
                label7.Text = "Maksymalna wartość prądu     Im: ";
            }

            chart1.ChartAreas[0].AxisY = ay;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Wybor_pliku();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "" )
            {
                MessageBox.Show("Wpisz wartość przekładni!");
            }
            else if(name_of_csv_file == "")
            {
                MessageBox.Show("Proszę wybrać plik");
            }
            else
            {
                Odczyt_pliku_i_obliczanie();
                Wykres(t, U);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            listView1.Clear();
            chart1.Series[0].Points.Clear();
            t.Clear();
            U.Clear();
            name_of_csv_file = "";
        }
    }
}
//sprawdzenie przycisków do wybrania rodzaju udaru