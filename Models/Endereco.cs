using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorViaCep.Models;

public class Endereco
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Rua { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string? Cidade { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("ibge")]
    public string? Ibge { get; set; }

    [JsonPropertyName("gia")]
    public string? Gia { get; set; }

    [JsonPropertyName("ddd")]
    public string? Ddd { get; set; }

    [JsonPropertyName("siafi")]
    public string? Siafi { get; set; }

    // O serviço pode enviar o indicador como booleano true ou texto "true".
    [JsonPropertyName("erro")]
    public JsonElement Erro { get; set; }

    [JsonIgnore]
    public bool NaoEncontrado => Erro.ValueKind == JsonValueKind.True
        || (Erro.ValueKind == JsonValueKind.String
            && bool.TryParse(Erro.GetString(), out var erro) && erro);
}
