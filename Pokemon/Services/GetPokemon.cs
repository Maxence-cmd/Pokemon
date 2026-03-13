using Newtonsoft.Json;
using Pokemon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Pokemon.Services
{
    public class GetPokemon
    {
        public async Task<Root> GetApiPokemon(string poke)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync("https://tyradex.app/api/v1/pokemon/"+poke);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Root pokeData = JsonConvert.DeserializeObject<Root>(jsonResponse);
                try
                {
                    pokeData = JsonConvert.DeserializeObject<Root>(jsonResponse);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur JSON : " + ex.Message);
                    return null;
                }


                return pokeData;
            }

            else
            {
                MessageBox.Show("Error fetching weather data.");
                return null;
            }
        }
    }

    public static class PokemonHelper
    {
        public static Random rnd = new Random();

        public static Root GetRandomPokemon(List<Root> poke)
        {
            if (poke == null || poke.Count == 0)
                return null;

            int index = rnd.Next(poke.Count);
            return poke[index];
        }

        public static BitmapImage GetPokemonImage(Root pokemon)
        {
            if (pokemon?.sprites?.regular != null)
            {
                return new BitmapImage(new Uri(pokemon.sprites.regular, UriKind.Absolute));
            }
            return null;
        }
    }
}
