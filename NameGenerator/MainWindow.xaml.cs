using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private HttpClient _client;
        public List<string> Names { get; set; }
        public List<string> Generated { get; set; }
        public List<Origins> Origins { get; set; }
        public int Order { get; set; }
        public int OutputCount { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Order = 2;
            OutputCount = 10;


            Names = new List<string>();
            Generated = new List<string>();
            Origins = new List<Origins>();

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://en.wikipedia.org/w/api.php");

            FillOrigins();
        }


        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            string CategoryName = "Surnames_of_French_origin";

            string query = "?cmtitle=Category:" + CategoryName + "&action=query&list=categorymembers&cmlimit=500&cmprop=title%7Csortkey%7Ctimestamp&format=json";

            HttpResponseMessage response = _client.GetAsync(query).Result;

            var rawlist = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result)["query"]["categorymembers"];

            foreach (var name in rawlist)
            {
                if ((int)name["ns"] == 0)
                {
                    Names.Add((((string)name["title"]).Split('(')[0]).Trim());
                }
            }

        }


        private void FillOrigins()
        {
            Origins.Add(new Origins("German", "German-language_surnames"));
            Origins.Add(new Origins("Italian", "Italian-language_surnames"));
            Origins.Add(new Origins("Russian", "Russian-language_surnames"));
            Origins.Add(new Origins("Jewish", "Jewish_surnames"));
            Origins.Add(new Origins("English", "English-language_surnames"));
            Origins.Add(new Origins("Serbian", "Serbian-language_surnames"));
            Origins.Add(new Origins("Dutch", "Dutch-language_surnames"));
            Origins.Add(new Origins("Dutch", "Surnames_of_Dutch_origin"));
            Origins.Add(new Origins("Polish", "Polish-language_surnames"));
            Origins.Add(new Origins("Turkish", "Turkish-language_surnames"));
            Origins.Add(new Origins("Indian", "Indian_family_names"));
            Origins.Add(new Origins("Arabic", "Arabic-language_surnames"));
            Origins.Add(new Origins("Spanish", "Spanish-language_surnames"));
            Origins.Add(new Origins("Romanian", "Romanian-language_surnames"));
            Origins.Add(new Origins("Swedish", "Swedish-language_surnames"));
            Origins.Add(new Origins("Irish", "Surnames_of_Irish_origin"));
            Origins.Add(new Origins("Yiddish", "Yiddish-language_surnames"));
            Origins.Add(new Origins("Croatian", "Croatian-language_surnames"));
            Origins.Add(new Origins("Finnish", "Finnish-language_surnames"));
            Origins.Add(new Origins("Czech", "Czech-language_surnames"));

            Origins.OrderBy(x => x.DisplayName);

            RaisePropertyChanged("Origins");
        }


        private Dictionary<string, Dictionary<string, int>> GetFrequencies()
        {
            Dictionary<string, Dictionary<string, int>> frequencies = new Dictionary<string, Dictionary<string, int>>();

            foreach (string n in Names)
            {
                for (int i = 0; i < n.Length - Order; i++)
                {
                    string seq = n.Substring(i, Order);
                    string letter = n.Substring(i + Order, 1);

                    if (!frequencies.ContainsKey(seq))
                    {
                        frequencies.Add(seq, new Dictionary<string, int>());
                    }
                    if (!frequencies[seq].ContainsKey(letter))
                    {
                        frequencies[seq].Add(letter, 1);
                    }
                    else
                    {
                        frequencies[seq][letter]++;
                    }
                }
            }

            return frequencies;
        }


        private void Generate()
        {
            Generated = new List<string>();
            Dictionary<string, int> firstChars = new Dictionary<string, int>();

            foreach (string n in Names)
            {
                string letter = n.Substring(0, 1);
                if (!firstChars.ContainsKey(letter))
                {
                    firstChars.Add(letter, 1);
                }
                else
                {
                    firstChars[letter]++;
                }
            }

            Dictionary<string, Dictionary<string, int>> frequencies = GetFrequencies();

            Random rnd = new Random();

            //picks a starting string for each generated name
            for (int i = 0; i < OutputCount; i++)
            {
                int randint = rnd.Next(0, Names.Count);

                foreach (string s in firstChars.Keys)
                {
                    string name = "";
                    if (randint < firstChars[s])
                    {
                        //string test = GenerateName(s);
                        //string test = s;
                        for (int j = 0; j < Order; j++)
                        {
                            int total = 0;
                            foreach (string myKey in frequencies.Keys.Where(st => st.Substring(0, 1) == s))
                            {
                                foreach (var t in frequencies[myKey])
                                {
                                    total += t.Value;
                                }
                            }

                            int randcont = rnd.Next(0, total);

                            foreach (string starter in frequencies.Keys.Where(st => st.Substring(0, 1) == s))
                            {
                                int subtotal = 0;
                                foreach (var t in frequencies[starter])
                                {
                                    subtotal += t.Value;
                                }
                                if (randcont < subtotal)
                                {
                                    name = starter;
                                    break;
                                }
                                randcont -= subtotal;
                            }
                        }

                        //Generated.Add(test);
                        Generated.Add(name);
                        break;
                    }
                    randint -= firstChars[s];
                }
            }


            //the rest of the name?
            for (int i = 0; i < Generated.Count; i++)
            {
                string name = Generated[i];
                string currStr = name.Substring(name.Length - Order);


                //TODO: find a better way of continuing the name if currStr is not in the dictionary
                while (currStr != "  " && frequencies.ContainsKey(currStr))
                {
                    int total = 0;
                    foreach (var d in frequencies[currStr])
                    {
                        total += d.Value;
                    }
                    int randint = rnd.Next(0, total);

                    foreach (var d in frequencies[currStr])
                    {
                        if (randint < total)
                        {
                            name += d.Key;
                            break;
                        }
                        randint -= total;
                    }

                    currStr = name.Substring(name.Length - Order);
                }

                Generated[i] = name.Trim();
            }

        }






        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }

    public class Origins: INotifyPropertyChanged
    {
        public string DisplayName { get; set; }
        public string SearchName;
        public bool IsChosen { get; set; }

        public Origins(string display, string search)
        {
            DisplayName = display;
            SearchName = search;
            IsChosen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

}
