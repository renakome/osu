// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online
{
    public class OsuDroidEndpointConfiguration : EndpointConfiguration
    {
        public OsuDroidEndpointConfiguration()
        {
            // Configure para conectar ao servidor osudroid-rx via ngrok
            WebsiteUrl = APIUrl = @"https://ornamentally-bibliophilistic-willard.ngrok-free.dev"; // URL do servidor ngrok
            APIClientSecret = Environment.GetEnvironmentVariable("API_CLIENT_SECRET") ?? ""; // Secret do servidor osudroid-rx
            APIClientID = Environment.GetEnvironmentVariable("API_CLIENT_ID") ?? ""; // ID do cliente para osudroid

            // Endpoints do osudroid (SignalR/WebSocket)
            SpectatorUrl = @"https://ornamentally-bibliophilistic-willard.ngrok-free.dev/spectator";
            MultiplayerUrl = @"https://ornamentally-bibliophilistic-willard.ngrok-free.dev/multiplayer";
            MetadataUrl = @"https://ornamentally-bibliophilistic-willard.ngrok-free.dev/metadata";

            // Beatmap submission service (se suportado)
            BeatmapSubmissionServiceUrl = @"https://ornamentally-bibliophilistic-willard.ngrok-free.dev";
        }
    }
}
