using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon.Services
{
    public class GererViePoke
    {
        private Random random = new Random();

        public int CalculerDegats(
            int niveau,
            int attaque,
            int defense,
            int puissance,
            double stab = 1.0,
            double efficacite = 1.0,
            bool critique = false)
        {
            // Facteur aléatoire entre 0.85 et 1
            double aleatoire = random.Next(85, 101) / 100.0;

            double critiqueBonus = critique ? 1.5 : 1.0;

            double modificateurs = stab * efficacite * critiqueBonus * aleatoire;

            double degats =
                (((2 * niveau / 5.0 + 2) * attaque * puissance / defense) / 50 + 2)
                * modificateurs;

            return (int)Math.Floor(degats);
        }

        public int AppliquerDegats(int pvActuel, int degats)
        {
            int nouveauPV = pvActuel - degats;

            if (nouveauPV < 0)
                nouveauPV = 0;

            return nouveauPV;
        }

        public bool EstKO(int pvActuel)
        {
            return pvActuel <= 0;
        }
    }
}

