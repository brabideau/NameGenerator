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
        public enum Gender { Male, Female, Either }

        public enum NameType { Given, Surname, Both }
        private HttpClient _client;

        public List<Gender> Genders { get; set; }
        public List<NameType> NameTypes { get; set; }

        public Gender ChosenGender { get; set; }
        public NameType ChosenNameType { get; set; }

        private List<string> SourceNames { get; set; }
        public List<string> Generated { get; set; }
        //public List<Origin> Origins { get; set; }
        public List<NameOrigin> Places { get; set; }
        public int Order { get; set; }
        public int OutputCount { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Order = 3;
            OutputCount = 20;

            SourceNames = new List<string>();
            Generated = new List<string>();
            Places = new List<NameOrigin>();

            Genders = Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();
            NameTypes = Enum.GetValues(typeof(NameType)).Cast<NameType>().ToList();

            ChosenGender = Genders[0];
            ChosenNameType = NameTypes[0];

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://en.wikipedia.org/w/api.php");

            FillPlaces();
        }


        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            //string sources = "Swedish-language_surnames";

            string sources = string.Join("|", Places.Where(p => p.IsChecked).Select(p => p.Surnames));          

            string query = String.Format("??action=query&generator=categorymembers&gcmlimit=500&format=json&gcmtitle=Category:{0}", sources);

            HttpResponseMessage response = _client.GetAsync(query).Result;

            var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

            while (result["continue"] != null)
            {
                string continuequery = string.Format(query + "&gcmcontinue= {0}", result["continue"]["gcmcontinue"]);
                var rawlist = result["query"]["categorymembers"];

                foreach (var name in rawlist)
                {
                    if ((int)name["ns"] == 0)
                    {
                        SourceNames.Add((((string)name["title"]).Split('(')[0]).Trim());
                    }
                }
            }
           

            SourceNames.RemoveAll(n => n.ToUpper().Contains("LIST OF"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("SURNAME"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("FAMILY"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("FAMILIES"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains(" NAME"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("CUSTOMS"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("DYNASTIES"));
            SourceNames.RemoveAll(n => n.ToUpper().Contains(" AND "));
            SourceNames.RemoveAll(n => n.ToUpper().Contains("CLAN"));

            Generate();
            RaisePropertyChanged("Generated");
        }


        private void FillPlaces()
        {
            Places.Add(new NameOrigin {
                                    PlaceName = "Arabic",
                                    Surnames = "Arabic-language_surnames",
                                    U_Names = "Arabic_given_names",
                                    F_Names = "Arabic_feminine_given_names",
                                    M_Names = "Arabic_masculine_given_names"  });

            Places.Add(new NameOrigin
            {
                PlaceName = "English",
                Surnames = "English-language_surnames|Surnames_of_English_origin|English_toponymic_surnames",
                U_Names = "English_unisex_given_names|English_given_names",
                F_Names = "English_feminine_given_names",
                M_Names = "English_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "German",
                Surnames = "German-language_surnames|Surnames_of_German_origin",
                U_Names = "German_given_names",
                F_Names = "German_feminine_given_names",
                M_Names = "German_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Hebrew",
                Surnames = "Hebrew-language_surnames",
                U_Names = "Hebrew_given_names",
                F_Names = "Hebrew_feminine_given_names",
                M_Names = "Hebrew_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Indian",
                Surnames = "Indian_family_names|Surnames_of_Indian_origin",
                U_Names = "Indian_unisex_given_names|Indian_given_names",
                F_Names = "Indian_feminine_given_names",
                M_Names = "Indian_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Italian",
                Surnames = "Italian-language_surnames|Surnames_of_Italian_origin",
                U_Names = "Italian_given_names",
                F_Names = "Italian_feminine_given_names",
                M_Names = "Italian_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Japanese",
                Surnames = "Japanese-language_surnames",
                U_Names = "Japanese_unisex_given_names",
                F_Names = "Japanese_feminine_given_names",
                M_Names = "Japanese_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Korean",
                Surnames = "Korean-language_surnames",
                U_Names = "Korean_unisex_given_names|Korean_given_names",
                F_Names = "Korean_feminine_given_names",
                M_Names = "Korean_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Portuguese",
                Surnames = "Portuguese-language_surnames",
                U_Names = "Portuguese_given_names",
                F_Names = "Portuguese_feminine_given_names",
                M_Names = "Portuguese_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Turkish",
                Surnames = "Turkish-language_surnames",
                U_Names = "Turkish_unisex_given_names|Turkish_given_names",
                F_Names = "Turkish_feminine_given_names",
                M_Names = "Turkish_masculine_given_names"
            });


            RaisePropertyChanged("Places");
        }



        private Dictionary<string, Dictionary<string, int>> GetFrequencies()
        {
            Dictionary<string, Dictionary<string, int>> frequencies = new Dictionary<string, Dictionary<string, int>>();

            foreach (string n in SourceNames)
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

            foreach (string n in SourceNames)
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
                int randint = rnd.Next(0, SourceNames.Count);

                foreach (string s in firstChars.Keys)
                {
                    string name = "";
                    if (randint < firstChars[s])
                    {

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
                while (currStr != "  " && frequencies.ContainsKey(currStr) && name.Length < 50)
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
                if (!Generated.Contains(name.Trim()))
                {
                    Generated.Add(name);
            }
        }

            Generated.Sort();
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
