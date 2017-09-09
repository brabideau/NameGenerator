using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace NameGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region properties and fields
        public enum Gender { Male, Female, Either }

        public enum NameType { Given, Surname, Both }
        private HttpClient _client;

        public List<Gender> Genders { get; set; }
        public List<NameType> NameTypes { get; set; }

        public Gender ChosenGender { get; set; }
        public NameType ChosenNameType { get; set; }

        private Random _random;
        private List<string> _sourceNames { get; set; }
        public List<string> Generated { get; set; }
        //public List<Origin> Origins { get; set; }
        public List<NameOrigin> Places { get; set; }
        public int Order { get; set; }
        private int _minLength;
        private int _maxLength;
        public int OutputCount { get; set; }

        private Dictionary<string, Dictionary<string, int>> _frequencies;

        private Dictionary<string, int> _firstChars;/* { get; set; }*/
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Order = 3;
            OutputCount = 20;

            _sourceNames = new List<string>();
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
            List<string> totalSources = Places.Where(p => p.IsChecked).Select(p => p.Surnames).ToList();
            _sourceNames = new List<string>();


            foreach(string source in totalSources)
            {
                string query = String.Format("?action=query&generator=categorymembers&gcmlimit=500&gcmtitle=Category:{0}&format=json", source);
                HttpResponseMessage response = _client.GetAsync(query).Result;

                var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                var rawlist = result["query"]["pages"];

                //TODO: Fix this it is dumb
                foreach (var page in rawlist)
                {
                    foreach(var name in page)
                    {
                        _sourceNames.Add((((string)name["title"]).Split('(')[0]).Trim());
                    }
                }


                while (result["batchcomplete"] == null)
                {
                    string continuequery = string.Format(query + "&gcmcontinue= {0}", result["continue"]["gcmcontinue"]);
                    rawlist = result["query"]["pages"];

                    foreach (var page in rawlist)
                    {
                        foreach (var name in page)
                        {
                            _sourceNames.Add((((string)name["title"]).Split('(')[0]).Trim());
                        }
                    }
                }

            }

            _sourceNames.RemoveAll(n => n.ToUpper().Contains("LIST OF"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("SURNAME"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("FAMILY"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("FAMILIES"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains(" NAME"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("CUSTOMS"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("DYNASTIES"));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains(" AND "));
            _sourceNames.RemoveAll(n => n.ToUpper().Contains("CLAN"));
            _sourceNames.RemoveAll(n => n.Length <= 1);

            GenerateList();
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
                Surnames = "English-language_surnames",
                //Surnames = "English-language_surnames|Surnames_of_English_origin|English_toponymic_surnames",
                U_Names = "English_unisex_given_names|English_given_names",
                F_Names = "English_feminine_given_names",
                M_Names = "English_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "German",
                Surnames = "German-language_surnames",
                //Surnames = "German-language_surnames|Surnames_of_German_origin",
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
                Surnames = "Indian_family_names",
                //Surnames = "Indian_family_names|Surnames_of_Indian_origin",
                U_Names = "Indian_unisex_given_names|Indian_given_names",
                F_Names = "Indian_feminine_given_names",
                M_Names = "Indian_masculine_given_names"
            });

            Places.Add(new NameOrigin
            {
                PlaceName = "Italian",
                Surnames = "Italian-language_surnames",
                //Surnames = "Italian-language_surnames|Surnames_of_Italian_origin",
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
                U_Names = "Korean_unisex_given_names",
                //U_Names = "Korean_unisex_given_names|Korean_given_names",
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
                U_Names = "Turkish_unisex_given_names",
                //U_Names = "Turkish_unisex_given_names|Turkish_given_names",
                F_Names = "Turkish_feminine_given_names",
                M_Names = "Turkish_masculine_given_names"
            });


            RaisePropertyChanged("Places");
        }


        private Dictionary<string, Dictionary<string, int>> GetFrequencies()
        {
            Dictionary<string, Dictionary<string, int>> frequencies = new Dictionary<string, Dictionary<string, int>>();

            foreach (string n in _sourceNames)
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


        private void GenerateList()
        {
            Order = 3;
            Generated = new List<string>();
            _firstChars = new Dictionary<string, int>();

            _minLength = _sourceNames.Min(n => n.Length);
            _maxLength = _sourceNames.Max(n => n.Length);

            if(Order <= _minLength) { Order = _minLength; }

            foreach (string n in _sourceNames)
            {
                string letter = n.Substring(0, 1);
                if (!_firstChars.ContainsKey(letter))
                {
                    _firstChars.Add(letter, 1);
                }
                else
                {
                    _firstChars[letter]++;
                }
            }

            _frequencies = GetFrequencies();

            _random = new Random();


            while(Generated.Count < OutputCount )
            {
                GenerateName();
            }

            Generated.Sort();
        }

        private void GenerateName()
        {
            int randint = _random.Next(0, _sourceNames.Count);
            string name = "";

            foreach (string s in _firstChars.Keys)
            {
                if (randint < _firstChars[s])
                {
                    for (int j = 0; j < Order; j++)
                    {
                        int total = 0;
                        foreach (string myKey in _frequencies.Keys.Where(st => st.Substring(0, 1) == s))
                        {
                            foreach (var t in _frequencies[myKey])
                            {
                                total += t.Value;
                            }
                        }

                        int randcont = _random.Next(0, total);

                        foreach (string starter in _frequencies.Keys.Where(st => st.Substring(0, 1) == s))
                        {
                            int subtotal = 0;
                            foreach (var t in _frequencies[starter])
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
                    break;
                }
                randint -= _firstChars[s];
            }
            


            //the rest of the name?


            string currStr = name.Substring(name.Length - Order);

            while (_frequencies.ContainsKey(currStr) && name.Length < _maxLength)
            {
                int total = 0;
                foreach (var d in _frequencies[currStr])
                {
                    total += d.Value;
                }
                randint = _random.Next(0, total);

                foreach (var d in _frequencies[currStr])
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

            if(name.Length > _minLength && !Generated.Contains(name)) { Generated.Add(name); }
            
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
