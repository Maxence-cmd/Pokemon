using Newtonsoft.Json;
using Pokemon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pokemon.Services
{
    public class GetPokemon
    {
        public async Task<Root> GetApiPokemon()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync("https://tyradex.app/api/v1/Pokemon");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Root weatherData = JsonConvert.DeserializeObject<Root>(jsonResponse);
                try
                {
                    weatherData = JsonConvert.DeserializeObject<Root>(jsonResponse);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur JSON : " + ex.Message);
                    return null;
                }


                return weatherData;
            }

            else
            {
                MessageBox.Show("Error fetching weather data.");
                return null;
            }
        }
    }
}
