using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon.Models
{
    internal class API
    {
    }
    public class Evolution
    {
        public List<Pre> pre { get; set; }
        public List<Next> next { get; set; }
        public List<Mega> mega { get; set; }
    }

    public class Forme
    {
        public string region { get; set; }
        public Name name { get; set; }
    }

    public class Gmax
    {
        public string regular { get; set; }
        public string shiny { get; set; }
    }

    public class Mega
    {
        public string orbe { get; set; }
        public Sprites sprites { get; set; }
    }

    public class Name
    {
        public string fr { get; set; }
        public string en { get; set; }
        public string jp { get; set; }
    }

    public class Next
    {
        public int pokedex_id { get; set; }
        public string name { get; set; }
        public string condition { get; set; }
    }

    public class Pre
    {
        public int pokedex_id { get; set; }
        public string name { get; set; }
        public string condition { get; set; }
        public string conditio { get; set; }
    }

    public class Resistance
    {
        public string name { get; set; }
        public double multiplier { get; set; }
    }

    public class Root
    {
        public int pokedex_id { get; set; }
        public int generation { get; set; }
        public string category { get; set; }
        public Name name { get; set; }
        public Sprites sprites { get; set; }
        public List<Type> types { get; set; }
        public List<Talent> talents { get; set; }
        public Stats stats { get; set; }
        public List<Resistance> resistances { get; set; }
        public Evolution evolution { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public List<string> egg_groups { get; set; }
        public Sexe sexe { get; set; }
        public int? catch_rate { get; set; }
        public int? level_100 { get; set; }
        public List<Forme> formes { get; set; }
        public object next { get; set; }
    }

    public class Sexe
    {
        public double male { get; set; }
        public double female { get; set; }
    }

    public class Sprites
    {
        public string regular { get; set; }
        public string shiny { get; set; }
        public Gmax gmax { get; set; }
    }

    public class Stats
    {
        public int hp { get; set; }
        public int atk { get; set; }
        public int def { get; set; }
        public int spe_atk { get; set; }
        public int spe_def { get; set; }
        public int vit { get; set; }
    }

    public class Talent
    {
        public string name { get; set; }
        public bool tc { get; set; }
    }

    public class Type
    {
        public string name { get; set; }
        public string image { get; set; }
    }

}
