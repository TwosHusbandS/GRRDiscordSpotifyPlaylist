<!--
Shamelessly stolen from: https://github.com/othneildrew/Best-README-Template
-->

<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Thanks again! Now go create something AMAZING! :D
-->

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->

[![Discord][discord-shield]][discord-url]
[![Twitter][twitter-shield]][twitter-url]
[![MIT License][license-shield]][license-url]
[![Maintained][maintained-shield]][maintained-url]

<!-- PROJECT LOGO -->
<br />
<p align="center">
  <!--<a href="https://github.com/TwosHusbandS/DasIstRaueberMusik">
    <img src="DIRM/Artwork/icon.png" alt="Logo" width="80" height="80">
  </a> -->

  <h3 align="center">GRR Discord Playlist Bot</h3>

  <p align="center">
    Gets YouTube and Spotify Links, as well as .fmbot Messages from a Discord Channel and converts them in a Spotify Playlist.
    <br />
    <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=PLACEHOLDER">View Demo</a>
	.
    <a href="#contact">Contact me</a>
  </p>
</p>

-----

## Built With

* Pretty much built with straight C#, dotnet 6.0, multiplatform
* Shoutout to [Google API .NET](https://github.com/googleapis/google-api-dotnet-client)
* Shoutout to [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET)
* Shoutout to [Discord.Net](https://github.com/discord-net/Discord.Net)
* May the pain these caused me (due to my own fault) help someone else lmfao.
* Did some small hackaround inside the project to get the ConfigFiles to copy to the correct directory. Unloud project to see. Prolly not best practice.


-----


<!-- ABOUT THE PROJECT -->
## About The Project

Builds a spotify Playlist based on Spotify and Youtube links posted to a specific Discord Channel.

-----

## Main Features

* Scrapes a Discord channel for Songs to add to a Spotify Playlist
* Can handle Spotify Links, Youtube Links, and [.fmbot](https://github.com/fmbot-discord/fmbot) .fm and .s Commands
* Logging
* Settings
* Limiting the amount of Songs the Playlist can have at most (deleting the oldest one when a new one is added and the limit is reached)
* Limiting the amount of Songs a User can add to the Playlist per day. Any further Songs (by that User on that day) will be ignored. This information is only kept in RAM and not saved to disk.
* Backups of all Songs in the Playlist

-----

## Options

### Config Possibilities:
* See [example config file](https://github.com/TwosHusbandS/GRRDiscordSpotifyPlaylist/blob/master/ConfigFiles/config.ini.example)
* SPOTIFY_CLIENT_ID (self explainatory)
* SPOTIFY_CLIENT_SECRET (actually not needed...I think)
* SPOTIFY_PLAYLIST_ID (Spotify ID of the Playlist we want to Change)
* SPOTIFY_PLAYLIST_NUMBER_OF_SONGS (Maximum number of Songs the Spotify Playlist should have.)
* YOUTUBE_API_KEY (self explainatory)
* DISCORD_OAUTH_CLIENT_ID (actually not needed...I think)
* DISCORD_OAUTH_CLIENT_SECRET (actually not needed...I think)
* DISCORD_BOT_TOKEN (self explainatory)
* DISCORD_GUILD_ID (ID of the Discord Server)
* DISCORD_INTERNAL_CHANNEL (ID of the Logging Channel)
* DISCORD_PUBLIC_CHANNEL (ID of the Channel we want to scrape for )
* EMBED_COLOR (Color of normal Embeds in Hex like "#FF00FF")
* LOG_COLOR (Color of logging Embeds in Hex like "#FF00FF")
* ERROR_COLOR (Color of error Embeds in Hex like "#FF00FF")
* USER_DAILY_LIMIT (Number of Songs a User can add per Day. Only kept in RAM, resets on restart)
* LOG_LEVEL_DISCORD (Loglevel to Discord, 1 = Info, 2 = Warnung, 3 = Error)
* LOG_LEVEL_FILE (Loglevel to Logfile, 1 = Info, 2 = Warnung, 3 = Error)
* SHOW_ACTIVITY_INTERNAL (True / False, logs each add/remove to an internal channel with reaction-commands)

### Discord Commands:
* /help
* /status
* /playlist

* /songs list
* /songs add {link}
* /songs remove {index}

* /backups list
* /backups create {name}
* /backups apply {name}
* /backups delete {name}

* /settings show
* /settings limit {limit_type} {number}
* /settings channel {channel_type} {name}
* /settings color {color_type} {name}
* /settings log {log_type} {log_level}
* /settings spotify_playlist_id {keep_songs} {playlist_id}
* /settings discord_guild_id {guild_id}
* /settings show-activity-internal {true/false}

* /dailylimitreset all
* /dailylimitreset user {user}


-----


## Contributing

* Feel free to PR, but I guess this is done


-----

## License

Distributed under the MIT License. See `LICENSE` for more information.

As long as you dont copy this 1:1 and charge money for it, we gucci.



-----

## Contact

Twitter - [@thsBizz][twitter-url]

Project Link - [github.com/TwosHusbandS/GRRDiscordSpotifyPlaylist][grrdiscordspotifyplaylist-url]

Discord - [@ths#0305][discord-url]


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[discord-url]: https://discordapp.com/users/612259615291342861
[twitter-url]: https://twitter.com/thSbizz
[grrdiscordspotifyplaylist-url]: https://github.com/TwosHusbandS/GRRDiscordSpotifyPlaylist
[twitter-shield]: https://img.shields.io/badge/Twitter-@thSbizz-1DA1F2?style=plastic&logo=Twitter
[discord-shield]: https://img.shields.io/badge/Discord-@thS%230305-7289DA?style=plastic&logo=Discord
[license-shield]: https://img.shields.io/badge/License-MIT-4DC71F?style=plastic
[license-url]: https://github.com/TwosHusbandS/GRRDiscordPlaylist/blob/master/LICENSE.md
[maintained-shield]: https://img.shields.io/badge/Maintained-Meh-FFDB3A?style=plastic
[maintained-url]: #Contributing


