using Newtonsoft.Json;

public class WeatherForecast
{
    public string? Cod { get; set; }
    public int Message { get; set; }
    public int Cnt { get; set; }
    public List<WeatherInfo>? List { get; set; }
}

public class WeatherInfo
{
    public long Dt { get; set; }
    [JsonProperty("dt_txt")]
    public DateTime DtTxt { get; set; }
    public Main? Main { get; set; }
    public List<Weather>? Weather { get; set; }
    public Clouds? Clouds { get; set; }
    public Wind? Wind { get; set; }
    public int Visibility { get; set; }

}

public class Main
{
    [JsonProperty("temp")]
    public double Temp { get; set; }

    [JsonProperty("feels_like")]
    public double FeelsLike { get; set; }

    [JsonProperty("temp_min")]
    public double TempMin { get; set; }

    [JsonProperty("temp_max")]
    public double TempMax { get; set; }

    [JsonProperty("pressure")]
    public int Pressure { get; set; }

    [JsonProperty("sea_level")]
    public int SeaLevel { get; set; }

    [JsonProperty("grnd_level")]
    public int GrndLevel { get; set; }

    [JsonProperty("humidity")]
    public double Humidity { get; set; }

    [JsonProperty("temp_kf")]
    public double TempKf { get; set; }
}

public class Weather
{
    public int Id { get; set; }
    public required string Main { get; set; }
    public required string Description { get; set; }
    public string? Icon { get; set; }
}

public class Clouds
{
    public int All { get; set; }
}

public class Wind
{
    public double Speed { get; set; }
    public int Deg { get; set; }
    public double Gust { get; set; }
}

