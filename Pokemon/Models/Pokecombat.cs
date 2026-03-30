using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon.Models
{
    public class PokemonCombat
    {
        public Root Pokemon { get; set; }

        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }

        public PokemonCombat(Root pokemon)
        {
            Pokemon = pokemon;
            MaxHP = pokemon.stats.hp;
            CurrentHP = MaxHP;
        }

        public bool IsDead()
        {
            return CurrentHP <= 0;
        }
    }
}
