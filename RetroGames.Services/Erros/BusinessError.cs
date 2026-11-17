using System.Net;

namespace RetroGames.Services.Erros;

/// <summary>
/// Um erro de negócio do catálogo: código (responseCode), mensagem e status HTTP.
/// O HttpStatusCode é só um valor guardado aqui; o Service não conhece HTTP.
/// </summary>
public sealed record BusinessError(int Code, string Message, HttpStatusCode Status);
