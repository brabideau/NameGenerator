using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

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

        private List<string> _genGiven;
        private List<string> _genSurnames;
        public List<NameOrigin> Places { get; set; }
        public int Order { get; set; }
        private int _minLength;
        private int _maxLength;
        public int OutputCount { get; set; }

        private Dictionary<string, Dictionary<string, int>> _frequencies;

        private Dictionary<string, int> _firstChars;
        #endregion

        public List<NameOriginInfo> MetaInfo { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        
            Order = 3;
            OutputCount = 20;
            Places = new List<NameOrigin>();

            _sourceNames = new List<string>();
            Generated = new List<string>();

            Genders = Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();
            NameTypes = Enum.GetValues(typeof(NameType)).Cast<NameType>().ToList();

            ChosenGender = Genders[0];
            ChosenNameType = NameTypes[0];

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://en.wikipedia.org/w/api.php");
            //MetaInfo = new List<NameOriginInfo>();

            FillPlaces();

            /*--------------------*/

            //List<NameOriginInfo> MetaInfo = new List<NameOriginInfo>();

            //foreach (NameOrigin n in Places)
            //{
            //    NameOriginInfo myMeta = new NameOriginInfo();
            //    myMeta.PlaceName = n.PlaceName;
            //    myMeta.SurnameCount = String.IsNullOrEmpty(n.Surnames) ? 0 :GetCount(n.Surnames);
            //    myMeta.U_NamesCount = String.IsNullOrEmpty(n.U_Names) ? 0 :GetCount(n.U_Names);
            //    myMeta.F_NamesCount = String.IsNullOrEmpty(n.F_Names) ? 0 :GetCount(n.F_Names);
            //    myMeta.M_NamesCount = String.IsNullOrEmpty(n.M_Names) ? 0 :GetCount(n.M_Names);

            //    MetaInfo.Add(myMeta);
            //}


            //RaisePropertyChanged("MetaInfo");
            //theGrid.DataContext = MetaInfo;

            /*--------------------*/

        }

        private int GetCount(string source)
        {

            if (string.IsNullOrEmpty(source))
            {
                return 0;
            }

            List<string> listOfNames = new List<string>();

            string query = String.Format("?action=query&generator=categorymembers&gcmlimit=500&gcmtitle=Category:{0}&format=json", source);
            HttpResponseMessage response = _client.GetAsync(query).Result;

            var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

            var rawlist = result["query"]["pages"];

            //TODO: Fix this it is dumb
            foreach (var page in rawlist)
            {
                foreach (var name in page)
                {
                    listOfNames.Add((((string)name["title"]).Split('(')[0]).Trim());
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
                        listOfNames.Add((((string)name["title"]).Split('(')[0]).Trim());
                    }
                }
            }

            listOfNames.RemoveAll(n => n.ToUpper().Contains("LIST OF"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("SURNAME"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("FAMILY"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("FAMILIES"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains(" NAME"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("CUSTOMS"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("DYNASTIES"));
            listOfNames.RemoveAll(n => n.ToUpper().Contains(" AND "));
            listOfNames.RemoveAll(n => n.ToUpper().Contains("CLAN"));
            listOfNames.RemoveAll(n => n.Length <= 1);

            return listOfNames.Count();
            

        }


        
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            List<string> totalSources = new List<string>();

            switch (ChosenNameType)
            {
                case NameType.Given:
                    totalSources = GetGenderedNames(totalSources);
                    break;

                case NameType.Surname:
                    totalSources = Places.Where(p => p.IsChecked).Select(p => p.Surnames).ToList();
                    break;

                case NameType.Both:
                    totalSources = Places.Where(p => p.IsChecked).Select(p => p.Surnames).ToList();
                    totalSources = GetGenderedNames(totalSources);
                    break;
            }


            totalSources.RemoveAll(s => string.IsNullOrEmpty(s));


            if(totalSources.Count > 0)
            {
                FillSources(totalSources);

                GenerateList();
                RaisePropertyChanged("Generated");
            }

        }


        private List<string> GetGenderedNames(List<string> totalSources)
        {
            switch (ChosenGender)
            {
                case Gender.Either:
                    totalSources = Places.Where(p => p.IsChecked).Select(p => p.U_Names).ToList();
                    totalSources = totalSources.Concat(Places.Where(p => p.IsChecked).Select(p => p.F_Names)).ToList();
                    totalSources = totalSources.Concat(Places.Where(p => p.IsChecked).Select(p => p.M_Names)).ToList();
                    break;

                case Gender.Male:
                    totalSources = Places.Where(p => p.IsChecked).Select(p => p.M_Names).ToList();

                    break;

                case Gender.Female:
                    totalSources = Places.Where(p => p.IsChecked).Select(p => p.F_Names).ToList();
                    break;

                default: break;
            }

            return totalSources;
        }

        private void FillSources(List<string> totalSources)
        {
            _sourceNames = new List<string>();

            foreach (string source in totalSources)
            {
                string query = String.Format("?action=query&generator=categorymembers&gcmlimit=500&gcmtitle=Category:{0}&format=json", source);
                HttpResponseMessage response = _client.GetAsync(query).Result;

                var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                var rawlist = result["query"]["pages"];

                //TODO: Fix this it is dumb
                foreach (var page in rawlist)
                {
                    foreach (var name in page)
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
        }

        private void FillPlaces()
        {
            Directory.SetCurrentDirectory(@"..\..");
            XmlSerializer n = new XmlSerializer(typeof(List<NameOrigin>), new XmlRootAttribute("Places"));

            Places = (List<NameOrigin>)n.Deserialize(new XmlTextReader("NameList.xml"));

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

            int attempts = 0;
            while(Generated.Count < OutputCount && attempts < OutputCount * 5)
            {
                GenerateName();
                attempts++;
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
