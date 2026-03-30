using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon.Services
{
    public class Attaque
    {   
    public string Nom { get; set; }
    public string Type { get; set; }
    public int Puissance { get; set; }

        public Attaque(string nom, string type, int puissance)
        {
            Nom = nom;
            Type = type;
            Puissance = puissance;
        }
    }
}
