using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pokemon.Models;

namespace Pokemon.Services
{
        public class GetIDFromName
        {
            public static async Task<PokemonNom[]> GetAllPokemon()
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://pokeapi.co/api/v2/pokemon?limit=2000";
                    var response = await client.GetStringAsync(url);
                    var data = JObject.Parse(response);

                    var results = data["results"];

                    var tableau = results.Select(item =>
                    {
                        string name = item["name"].ToString();
                        string urlPokemon = item["url"].ToString();
                        var parts = urlPokemon.Split('/');
                        int id = int.Parse(parts[^2]);

                        return new PokemonNom
                        {
                            Nom = name,
                            Id = id
                        };
                    }).ToArray();

                    return tableau;
                }
            }
        }
    }
