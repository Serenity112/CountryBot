namespace CountryBot
{
    class CountryData
    {
        public CountryData(string capital)
        {
            this.capital = capital;
        }
        public string capital { get; set; }
        public string language { get; set; }
        public string currency { get; set; }
    }
}
