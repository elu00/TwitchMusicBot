# TwitchMusicBot
A twitch chat bot that can parse song requests from users and display an appropriate list of songs.

### Current Project Status: Beta

Built heavily around [TwitchLib](https://github.com/swiftyspiffy/TwitchLib) Huge thanks to the developers of this project!  
Developed by Ethan Lu; contact me at elu@nevada.unr.edu

## Features:

- Fully featured Material Design GUI built in WPF, with full display and visualization of the song list
- Full integration into twitch chat, with all commands fully supported.
- Mod filtering for sensitive or major commands.

## Commands:

- !request \<song\>
- !spot
- !remove
- !list
- !next
- !commands
- !change \<url\>
- !currentsong

Most of the basic functionality has been implemented already, testing and debugging will be continuing on soon.  

## Planned features:

- Support for parsing title data from soundcloud and youtube urls (coming next)
- Integrating the twitch alerts API for donation tracking and priortization.
- Limiting requests per user per stream to >1
- Ability to whisper requestees their spot in the queue
- Recording video length and performance length for various analytics
- Moderator functions to moderate the queue
- Server capabilities for easy visualization of the list

## Credits  
Thanks to the authors of [TwitchLib](https://github.com/swiftyspiffy/TwitchLib) and [MaterialDesigninXAML](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)!
